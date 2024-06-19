# 1. Data preprocessing


## 0. 데이터 로딩 
~~~python
import pandas as pd
import torch

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print('Device:', device)
data_origin = pd.read_csv("../trains/Dataset.csv")
~~~


## 1. 데이터의 분산를 파악하는 코드 
1. 손이 떨리는 경우
2. 손이 안떨리는 경우
3. 전체 데이터 분산
~~~python
import matplotlib.pyplot as plt
import numpy as np

# 3 by 3 그래프를 위한 데이터 생성
# 3 by 3 서브플롯 그리기

fig, axs = plt.subplots(3, 3, figsize=(15, 15))

# 각 서브플롯에 히스토그램 그리기
for i, ax in enumerate(axs.flat):
    data_set = data[data_col[i]] # (전체 데이터 분산)
    #data_set = data.loc[mask_2, data_col[i]] (손 떨림 있는 데이터)
    # data_set = data.loc[mask_1, data_col[i]] (손 떨림 없는 데이터)
    ax.hist(data_set, bins=30, color='skyblue', edgecolor='black')
    ax.set_title(f'Data Set {data_col[i]}')
    ax.set_xlabel('Value')
    ax.set_ylabel('count')

# 레이아웃 조정
plt.tight_layout()
plt.show()
~~~

## 2. 데이터 이상치값 판단 코드
~~~python
import seaborn as sns
print(sns.boxplot(data['aX']))
~~~

# 2. HW 구축 
## 1. MPU9250 Low 데이터에서 High 변환
~~~
  // Read accelerometer and gyroscope
  uint8_t Buf[14];
  I2Cread(MPU9250_ADDRESS, 0x3B, 14, Buf);
      
  // Accelerometer
  int16_t ax = -(Buf[0] << 8 | Buf[1]);
  int16_t ay = -(Buf[2] << 8 | Buf[3]);
  int16_t az = Buf[4] << 8 | Buf[5];

  // Gyroscope
  int16_t gx = -(Buf[8] << 8 | Buf[9]);
  int16_t gy = -(Buf[10] << 8 | Buf[11]);
  int16_t gz = Buf[12] << 8 | Buf[13];

  
  // _____________________
  // :::  Magnetometer ::: 
  uint8_t ST1;
  do
  {
    I2Cread(MAG_ADDRESS, 0x02, 1, &ST1);
  }
  while (!(ST1 & 0x01));

  // Read magnetometer data  
  uint8_t Mag[7];  
  I2Cread(MAG_ADDRESS, 0x03, 7, Mag);
  
  // Magnetometer
  int16_t mx = -(Mag[3] << 8 | Mag[2]);
  int16_t my = -(Mag[1] << 8 | Mag[0]);
  int16_t mz = -(Mag[5] << 8 | Mag[4]);
~~~


## 2. 진동 모듈 구현 
1. python 통신 중에 1을 받으면 진동 모듈 ON
2. python 통신 중에 0을 받으면 진동 모듈 OFF
~~~
while (!intFlag);
intFlag = false;
delay(500);

if (Serial.available() > 0) {
    char receivedChar = Serial.read();
    
    if (receivedChar == '1') {
      for(int i=0;i<=255;i++){
          analogWrite(11,i);
        }
    }else{
      for(int i=0;i<=0;i++){
          analogWrite(11,i);
        }
    }
  }
~~~

# 3. AI Model
## 3.1 시계열 데이터 변환
1. 5개 데이터를 주기로 병합하는 과정
~~~python
def sliding_windows(data, seq_length):
    x = []
    y = []
    print(len(data)-seq_length-1)
    for i in range(len(data)-seq_length-1):
        _x = data[i:(i+seq_length)]
        _y = data_origin.iloc[i+seq_length, -1]
        x.append(_x)
        y.append(_y)

    return np.array(x),np.array(y)

seq_length = 5
~~~


## 3.2 train/test data 분할(80% & 20%)
~~~python
train_size = int((3 / 4) * len(y))
test_size = len(y) - train_size

X_data = torch.Tensor(np.array(x))
Y_data = torch.Tensor(np.array(y))
X_data = X_data.to(device)
Y_data = Y_data.to(device)

X_train = torch.Tensor(np.array(x[0:train_size])).to(device)
Y_train = torch.Tensor(np.array(y[0:train_size])).to(device)

X_test = torch.Tensor(np.array(x[train_size:len(x)])).to(device)
Y_test = torch.Tensor(np.array(y[train_size:len(y)])).to(device)
~~~


## 3.3 Model  architecture
1. LSTM 모델 구성
2. 최종 class는 이진 분류
3. input_size는 feature 사이즈와 동일
4. seq_length 5로 고정

~~~python
class LSTM(nn.Module):

    def __init__(self, num_classes, input_size, hidden_size, num_layers, device):
        super(LSTM, self).__init__()
        seq_length = 5
        self.num_classes = num_classes
        self.num_layers = num_layers
        self.input_size = input_size
        self.hidden_size = hidden_size
        self.seq_length = seq_length
        self.device = device

        self.lstm = nn.LSTM(input_size=input_size, hidden_size=hidden_size,
                            num_layers=num_layers, batch_first=True)

        self.fc = nn.Linear(hidden_size, num_classes)

    def forward(self, x):
        h_0 = torch.zeros(
            self.num_layers, x.size(0), self.hidden_size).to(self.device)

        c_0 = torch.zeros(
            self.num_layers, x.size(0), self.hidden_size).to(self.device)

        # Propagate input through LSTM
        output, (h_n, c_n) = self.lstm(x, (h_0, c_0))

        h_n = h_n.view(-1, self.hidden_size).to(self.device)

        result = self.fc(h_n)

        return result
~~~

## 3.4 Trainer 방법
1. Trainer 안에 train & evaluate 동시에 실행
2. 최적의 validation loss 찾아 모델 파라미터 저장

~~~python
def trainer(model, num_epoch, dataloader_dict, optim, criterion, early_stop, device):
    EPOCHS = num_epoch
    lowest_epoch = 0
    best_valid_loss = float('inf')
    train_history, valid_history = [], []
    for epoch in range(EPOCHS):
        start_time = time.monotonic()
        train_loss = train(model, dataloader_dict['train'], optim, criterion, device)
        valid_loss = evaluate(model, dataloader_dict['valid'], criterion, device)
        print(valid_loss, train_loss)
        if valid_loss < best_valid_loss:
            lowest_epoch = epoch
            best_valid_loss = valid_loss
            torch.save(model.state_dict(), f'save.pt')
        if early_stop > 0 and lowest_epoch + early_stop < epoch + 1:
            print("There is no improvement during last %d epochs." % early_stop)
            break
        train_history.append(train_loss)
        valid_history.append(valid_loss)

        end_time = time.monotonic()
        epoch_mins, epoch_secs = epoch_time(start_time, end_time)

        print(f'Epoch: {epoch + 1:02} | Epoch Time: {epoch_mins}m {epoch_secs}s')
        print(f'\tTrain Loss: {train_loss:.3f}')
        print(f'\t Valid. Loss: {valid_loss:.3f}')

    return train_history, valid_history

~~~


## 3.4 Train method
1. 모델을 훈련하기 때문에 model.train() 설정(backward 실행)
2. 최종 Loss값 반환

~~~python
def train(model, data_loader, optim, criterion, device):
    model.train()
    losses = 0
    loss_list = []
    for d, v in tqdm(data_loader):
        data = d.to(device)
        v = v.to(device)
        optim.zero_grad()
        output_data = model(data)

        result = F.sigmoid(output_data)
        loss = criterion(result.reshape(-1, 1).to(torch.float64), v.reshape(-1, 1).to(torch.float64))
        loss_list.append(loss.item())

        loss.backward()
        optim.step()
        losses += loss.item()

    return losses / len(data_loader)
~~~


## 3.5 evaluate method
1. 데이터를 평가하기 위한 함수
2. model.eval() 설정(backward 실행 없음)
~~~python
def evaluate(model, dataloader, criterion, device):
    epoch_loss = 0

    model.eval()
    with torch.no_grad():
        for d, v in tqdm(dataloader):

            data = d.to(device)
            v = v.to(device)

            output_data = model(data)
            result = F.sigmoid(output_data)
            loss = criterion(result.reshape(-1, 1).to(torch.float64), v.reshape(-1, 1).to(torch.float64))
            epoch_loss += loss.item()

    return epoch_loss / len(dataloader)
~~~

## 3.6 test method
1. 기존에는 accuracy 측정 대신 Loss 측정
2. Test Method 에서는 accuracy 측정
~~~
lstm.eval()
loss_history = []

model = LSTM(num_classes, input_size, hidden_size, num_layers, device)

model.load_state_dict(torch.load('save.pt'))
model.eval()  # 모델을 평가 모드로 설정합니다.
model.to(device)


with torch.no_grad():
    for i, (d, v) in enumerate(valid_data_loader):
        data = d
        v = v
        output_data = model(data)
        result = F.sigmoid(output_data)
        rounded_preds = torch.round(result)
        correct = (rounded_preds.flatten() == v)
        accuracy = correct.sum() / len(correct)
        print(f"{i}정확도 : {accuracy*100}%")
        loss = loss_function(result.reshape(-1, 1).to(torch.float32), v.reshape(-1, 1).to(torch.float32))
        loss_history.append(loss.item())
        epoch_loss += loss.item()
~~~


# 4. 통신하는 방법
## 4.1 Config 구성
1. 실험 환경과 동일한 환경으로 input_size, hidden_size 구성
2. load_state_dict 통해 학습 파라미터 로딩

~~~python
py_serial = serial.Serial(
    # Window
    port='COM8',
    # 보드 레이트 (통신 속도)
    baudrate=115200,
)

input_size = 6
hidden_size = 128
num_layers = 1
num_classes = 1

lstm = LSTM(num_classes, input_size, hidden_size, num_layers)
lstm.load_state_dict(torch.load('./trains/save_lstm.pt'))

linear = False
data_list = deque([])

i = 0
time.sleep(1)
~~~

## 4.2 Linear model loading
1. Linear model 가져와서 사용할 때 사용
2. 본 코드는 손 떨림 데이터의 움직임을 체크하는 것이 아닌, 단순 순의 위치를 파악할 때 사용

~~~python
if linear == True:
    if py_serial.readable():
        # 들어온 값이 있으면 값을 한 줄 읽음 (BYTE 단위로 받은 상태)
        # BYTE 단위로 받은 response 모습 : b'\xec\x97\x86\xec\x9d\x8c\r\n'
        response = py_serial.readline()
        print(response)
        # 디코딩 후, 출력 (가장 끝의 \n을 없애주기위해 슬라이싱 사용)
        data = response[:len(response) - 1].decode()
        data = data.replace("end\r", "")
        values = data.split(', ')
        try:
            print(values)
            Axyz = [float(value) for value in values[0:3]]
            Gxyz = [float(value) for value in values[3:6]]
            # Mxyz = [float(value) for value in values[6:7]]
            combined_list = []
            combined_list.extend(Axyz)
            combined_list.extend(Gxyz)
            # combined_list.extend(Mxyz)
            data = (torch.tensor(combined_list, dtype=torch.float64))
            print("실행 결과 : ", data)
            output_data = model(data)
            result = F.sigmoid(output_data)
            mask = (result >= torch.FloatTensor([0.5]))
            if mask:
                print("실행ㅉ")
                c = "1"
                c = c.encode('utf-8')
                print(py_serial.write(c))
            else:
                print("거짓")
                c = "0"
                c = c.encode('utf-8')
                print(py_serial.write(c))
        except Exception as e:
            print(e)
~~~

## 4.3 LSTM model loading
1. LSTM 시계열 모델 사용
2. linear 방식이 아닌, LSTM 통해 손에 움직임을 파악때 사용

~~~python
else:
    response = py_serial.readline()
    print(response)
    # 디코딩 후, 출력 (가장 끝의 \n을 없애주기위해 슬라이싱 사용)
    data = response[:len(response) - 1].decode()
    data = data.replace("end\r", "")
    values = data.split(', ')
    Axyz = [float(value) for value in values[0:3]]
    Gxyz = [float(value) for value in values[3:6]]
    combined_list = []
    combined_list.extend(Axyz)
    combined_list.extend(Gxyz)
    if len(data_list) < 5:
        data_list.append(combined_list)
        continue
    else:
        data = (torch.tensor(data_list, dtype=torch.float32)).reshape(1, -1, 6)
        output_data = lstm(data)
        result = F.sigmoid(output_data)
        mask = (result >= torch.FloatTensor([0.5]))
        if mask:
            print("True")
            c = "1"
            c = c.encode('utf-8')
            py_serial.write(c)
            print(py_serial.write(c))
        else:
            print("False")
            c = "0"
            c = c.encode('utf-8')
            py_serial.write(c)
    data_list = modify_queue(data_list, combined_list)
~~~


## 5. Build 
~~~
python MPU_connect.py
~~~