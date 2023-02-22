如何配置服务器？
必要软件：golang，redis，nginx
启动步骤：
1、替换nginx.conf到本地nginx安装目录下并运行nginx -s reload 以重载配置
2、打开终端输入redis-server以启动redis
3、打开三个终端，分别定位到LogicServer、RoomCenterServer、RoomServer，在三个终端分别输入go run LogicServer/RoomCenterServer/RoomServer.go 以运行服务器
4、打开游戏，在设置界面中输入服务器的IP地址
5、开始游玩！

如果不启动服务器也可游玩，不过是离线模式