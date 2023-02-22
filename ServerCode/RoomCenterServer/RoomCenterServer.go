package main

import (
	"context"
	//"fmt"
	"net"
	"net/rpc"

	"github.com/go-redis/redis/v9"
)

var rdb = redis.NewClient(&redis.Options{
	Addr:     "localhost:6379",
	Password: "",
	DB:       0,
})

var ctx = context.Background()

type RoomCenterService struct{}

func (s *RoomCenterService) Start(id string, reply *string) error {
	value, err := rdb.SRandMember(ctx, "RoomServerAddressSet").Result()
	if err != nil {
		panic(err)
	}
	*reply = value
	return nil
}

func main() {
	rpc.RegisterName("RoomCenterService", new(RoomCenterService))

	listener, err := net.Listen("tcp", ":1200")
	if err != nil {
		panic(err)
	}

	for {
		conn, err := listener.Accept()
		if err != nil {
			panic(err)
		}

		rpc.ServeConn(conn)
	}
}
