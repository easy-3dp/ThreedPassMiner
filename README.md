[![logo](https://3dpass.org/assets/img/3DPass_on_the_moon.png)](https://3dpass.org)

# 一些链接
官网 https://3dpass.org/  
官网钱包 https://wallet.3dpass.org/  
官网挖矿遥测 https://telemetry.3dpass.org/#/0x6c5894837ad89b6d92b114a2fb3eafa8fe3d26a54848e3447015442cd6ef4e66  
官网挖矿数据 https://explorer2.3dpass.org/blocks.php  
挖矿统计 https://3dp.zhuaao.com/  

# 社区
Discord https://discord.com/invite/u24WkXcwug  
Telegram https://t.me/pass3d  
中文社区 QQ群 692365694

# links
Official website https://3dpass.org/  
Official website wallet https://wallet.3dpass.org/  
Official website mining telemetry https://telemetry.3dpass.org/#/0x6c5894837ad89b6d92b114a2fb3eafa8fe3d26a54848e3447015442cd6ef4e66  
Official website mining data https://explorer2.3dpass.org/blocks.php  
Mining Statistics https://3dp.zhuaao.com/  

# Community
Discord https://discord.com/invite/u24WkXcwug  
Telegram https://t.me/pass3d  

---

# 轻松3DP矿工

轻松3DP矿工是3DP主网的挖矿程序，支持Windows和Linux系统。

# 教程

## 运行此程序需要安装.net6.0。

### Ubuntu系统安装方法：

获取ROOT权限
```sh
sudo -i
```
apt更新
```sh
apt update
```

安装.net6
```sh
apt install -y wget
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt update && apt install -y dotnet-sdk-6.0
```

### Windows系统安装方法：
官网下载程序安装，下载地址：https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.7-windows-x64-installer


## 使用方法
Linux
```sh
dotnet ThreedPassMinerDeamon.dll
```

Windows
```sh
ThreedPassMinerDeamon.exe
```
可选参数：

使用多少线程，默认是逻辑处理器*4
```sh
--threads 64
```

连接的节点程序的IP，本机是127.0.0.1
```sh
--node-rpc-host 127.0.0.1
```

连接的节点程序的端口，就是节点程序参数里的--rpc-port
```sh
--node-rpc-port 9933
```

界面刷新的时间，默认1000毫秒（1秒==1000毫秒）
```sh
--refresh-interval 1000
```

如果你想测试算力：
```sh
dotnet ThreedPassMiner.dll --test --difficulty 100000
```

difficulty 后面填写正整数
```sh
--difficulty 100000
```

或者从节点的输出里复制，“Difficulty: []” 括号内文字为参数，需要加双引号
```sh
--difficulty "156, 94, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0"
```


WIN便捷运行方法：  
每次都要输入命令很麻烦，可以使用bat来自动化运行

新建文本文档，输入以下文字，保存，修改后缀为bat，参数根据自己情况修改
```sh
ThreedPassMinerDeamon.exe --threads 64 --node-rpc-host 127.0.0.1 --node-rpc-port 9933
```

## RockObjParams.json
调整生成模型的随机值。在官方代码中，它存在于miner\libs\rock_obj.js。  
这些参数的意义请自行阅读代码理解，这里不阐述。

---

# easy 3DP miner
easy_3DP_miner is the mining program of the 3DP mainnet, which supports Windows and Linux systems.

# Tutorial

## This program requires .net6.0.

### Ubuntu install tutorial:

Get ROOT permission
```sh
sudo -i
```
Apt update
```sh
apt update
```

Install .net6
```sh
apt install -y wget
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt update && apt install -y dotnet-sdk-6.0
```

### Windows install tutorial:
Official website download program installation, download link：https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.7-windows-x64-installer

## How to use

USAGE:

Linux
```sh
dotnet ThreedPassMinerDeamon.dll
```

Windows
```sh
ThreedPassMinerDeamon.exe
```
OPTIONS:

How many threads to use, the default is logical processor * 4
```sh
--threads 64
```

The IP of the connected node program, the local machine is 127.0.0.1
```sh
--node-rpc-host 127.0.0.1
```

The port of the connected node program, it is the '--rpc-port' in the node program parameters
```sh
--node-rpc-port 9933
```

The interface refresh interval, the default is 1000 milliseconds (1 second == 1000 milliseconds)
```sh
--refresh-interval 1000
```

If you want to test computing power：
```sh
dotnet ThreedPassMiner.dll --test --difficulty 100000
```

Fill in a positive integer after difficulty
```sh
--difficulty 100000
```

Or copy from the output of the node program, "Difficulty: []", the text in parentheses is a parameter and needs to be enclosed in double quotation marks
```sh
--difficulty "156, 94, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0"
```


WIN convenient operation method：  
It is very troublesome to enter the command every time, you can use bat to automate the operation

Create a new text document, enter the following text, save, modify the suffix to bat, and modify the parameters according to your own situation
```sh
ThreedPassMinerDeamon.exe --threads 64 --node-rpc-host 127.0.0.1 --node-rpc-port 9933
```

## RockObjParams.json
Adjust the random values for the generative model. In the official code, it exists in 'miner\libs\rock_obj.js'.  
Please read the code to understand the meaning of these parameters, and will not be explained here.