package main

import (
	"fmt"
	"io"
	"log"
	"net"

	pb "room_message"

	"google.golang.org/protobuf/proto"
)

type client chan<- [2][]byte

type playerInfo struct {
	id        int32
	positionX float32
	positionY float32
	positionZ float32
	forwardX  float32
	forwardY  float32
	forwardZ  float32
	curHealth float32
	maxHealth float32
}

var (
	entering         = make(chan client)
	leaving          = make(chan client)
	messages         = make(chan [2][]byte)
	playerInfoMap    = make(map[int32]*playerInfo)
	playerInfoAdd    = make(chan playerInfo)
	playerInfoRemove = make(chan int32)
)

func broadcaster() {
	clients := make(map[client]bool)
	for {
		select {
		case msg := <-messages:
			for cli := range clients {
				cli <- msg
			}

		case cli := <-entering:
			clients[cli] = true

		case cli := <-leaving:
			delete(clients, cli)
			close(cli)
		}
	}
}

func playerInfoHandler() {
	for {
		select {
		case playerInfo := <-playerInfoAdd:
			playerInfoMap[playerInfo.id] = &playerInfo
		case id := <-playerInfoRemove:
			delete(playerInfoMap, id)
		}
	}
}

func process(conn net.Conn) {

	fmt.Println("connected with " + conn.RemoteAddr().String())
	ch := make(chan [2][]byte, 1)
	readCh := make(chan []byte, 10)
	disconnected := false
	var id int32

	go func(conn net.Conn, ch <-chan [2][]byte) {
		for {
			msg := <-ch
			if disconnected {
				break
			}
			conn.Write([]byte{byte(len(msg[0]))})
			conn.Write(msg[0])
			conn.Write([]byte{byte(len(msg[1]))})
			conn.Write(msg[1])
		}
	}(conn, ch)

	go func(conn net.Conn, ch chan<- []byte) {
		readBuffer := make([]byte, 256)
		defer conn.Close()
		for {
			readLen, err := conn.Read(readBuffer[:])
			if err == io.EOF {
				break
			} else if err != nil {
				fmt.Println(err)
				break
			}

			if !disconnected {
				ch <- readBuffer[:readLen]
			}
		}
		close(ch)
		disconnected = true
	}(conn, readCh)

	entering <- ch

	var buffer []byte
	totalLen := 0
	var err error

	for !disconnected {

		if totalLen == 0 {
			tmpbuffer := <-readCh
			buffer = tmpbuffer
			totalLen = len(tmpbuffer)
		}
		if disconnected {
			break
		}

		typeBufLen := int(buffer[0])
		if totalLen < typeBufLen+2 {
			fmt.Println("not enough data")
			fmt.Printf("id: %d total: %d\n", id, totalLen)
			fmt.Printf("id: %d type: %d\n", id, typeBufLen)
			fmt.Println(buffer)
			totalLen = 0
			buffer = buffer[0:0]
			continue
		}

		messageType := &pb.MsgType{}
		if err = proto.Unmarshal(buffer[1:typeBufLen+1], messageType); err != nil {
			fmt.Println(err)
			fmt.Printf("id: %d total: %d\n", id, totalLen)
			fmt.Printf("id: %d type: %d\n", id, typeBufLen)
			fmt.Println(buffer)
			totalLen = 0
			buffer = buffer[0:0]
			continue
		}

		msgBufLen := int(buffer[typeBufLen+1])
		if totalLen < typeBufLen+msgBufLen+2 {
			fmt.Println("not enough data")
			fmt.Printf("id: %d total: %d\n", id, totalLen)
			fmt.Printf("id: %d type: %d\n", id, typeBufLen)
			fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
			fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
			fmt.Println(buffer)
			totalLen = 0
			buffer = buffer[0:0]
			continue
		}

		switch messageType.MsgType {
		case pb.MessageType_PlayerEnter:
			input := &pb.MsgPlayerEnter{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}

			for pID, pInfo := range playerInfoMap {
				outType := pb.MsgType{
					Id:      pID,
					MsgType: pb.MessageType_PlayerEnter,
				}
				outTypeBuf, err := proto.Marshal(&outType)
				if err != nil {
					log.Fatal(err)
				}
				outMsg := pb.MsgPlayerEnter{
					Id:        pID,
					PositionX: pInfo.positionX,
					PositionY: pInfo.positionY,
					PositionZ: pInfo.positionZ,
					ForwardX:  pInfo.forwardX,
					ForwardY:  pInfo.forwardY,
					ForwardZ:  pInfo.forwardZ,
					CurHealth: pInfo.curHealth,
					MaxHealth: pInfo.maxHealth,
				}
				outMsgBuf, err := proto.Marshal(&outMsg)
				if err != nil {
					log.Fatal(err)
				}
				ch <- [2][]byte{outTypeBuf[:], outMsgBuf[:]}
			}
			var info playerInfo
			info.id = input.Id
			info.positionX = input.PositionX
			info.positionY = input.PositionY
			info.positionZ = input.PositionZ
			info.forwardX = input.ForwardX
			info.forwardY = input.ForwardY
			info.forwardZ = input.ForwardZ
			info.curHealth = input.CurHealth
			info.maxHealth = input.MaxHealth
			id = input.Id
			playerInfoAdd <- info

			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		case pb.MessageType_PlayerMove:
			input := &pb.MsgPlayerMove{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}
			playerInfoMap[input.Id].positionX = input.PositionX
			playerInfoMap[input.Id].positionY = input.PositionY
			playerInfoMap[input.Id].positionZ = input.PositionZ
			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		case pb.MessageType_PlayerRotate:
			input := &pb.MsgPlayerRotate{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}
			playerInfoMap[input.Id].forwardX = input.ForwardX
			playerInfoMap[input.Id].forwardY = input.ForwardY
			playerInfoMap[input.Id].forwardZ = input.ForwardZ
			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		case pb.MessageType_PlayerStop:
			input := &pb.MsgPlayerStop{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}
			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		case pb.MessageType_PlayerShoot:
			input := &pb.MsgPlayerShoot{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}
			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		case pb.MessageType_PlayerSetHealth:
			input := &pb.MsgPlayerSetHealth{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}
			playerInfoMap[input.Id].curHealth = input.CurHealth
			playerInfoMap[input.Id].maxHealth = input.MaxHealth
			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		case pb.MessageType_PlayerExit:
			input := &pb.MsgPlayerExit{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}
			disconnected = true
			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		case pb.MessageType_PlayerDeath:
			input := &pb.MsgPlayerDeath{}
			if err = proto.Unmarshal(buffer[typeBufLen+2:typeBufLen+msgBufLen+2], input); err != nil {
				fmt.Println(err)
				fmt.Printf("id: %d total: %d\n", id, totalLen)
				fmt.Printf("id: %d type: %d\n", id, typeBufLen)
				fmt.Printf("id: %d %s\n", id, messageType.GetMsgType())
				fmt.Printf("id: %d msg: %d\n", id, msgBufLen)
				fmt.Println(buffer)
				totalLen = 0
				buffer = buffer[0:0]
				continue
			}
			messages <- [2][]byte{buffer[1 : typeBufLen+1], buffer[typeBufLen+2 : typeBufLen+msgBufLen+2]}

		}

		if totalLen > 0 {
			totalLen -= (typeBufLen + msgBufLen + 2)
			buffer = buffer[typeBufLen+msgBufLen+2:]
		}
	}

	fmt.Println("disconnected with " + conn.RemoteAddr().String())
	playerInfoRemove <- id
	leaving <- ch
}

func main() {
	listener, err := net.Listen("tcp", ":1201")
	if err != nil {
		log.Fatal(err)
	}

	go broadcaster()
	go playerInfoHandler()
	for {
		conn, err := listener.Accept()
		if err != nil {
			log.Fatal(err)
		}
		go process(conn)
	}
}
