module room_server

go 1.18

require room_message v0.0.0

require google.golang.org/protobuf v1.28.1 // direct

replace room_message => ../Room_Client
