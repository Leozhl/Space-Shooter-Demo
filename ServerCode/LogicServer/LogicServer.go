package main

import (
	"context"
	"fmt"
	"net/http"
	"net/rpc"

	"strconv"

	"github.com/go-redis/redis/v9"
)

var rdb = redis.NewClient(&redis.Options{
	Addr:     "localhost:6379",
	Password: "",
	DB:       0,
})

var ctx = context.Background()

func main() {
	http.HandleFunc("/setID", setIDHandler)
	http.HandleFunc("/setName", setNameHandler)
	http.HandleFunc("/getName", getNameHandler)
	http.HandleFunc("/ranking", rankingHandler)
	http.HandleFunc("/setRanking", setRankingHandler)
	http.HandleFunc("/start", startHandler)

	defer rdb.Close()
	defer rdb.Save(ctx)
	http.ListenAndServe("localhost:8080", nil)
}

func setIDHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	data := r.URL.Query()
	cn := rdb.Conn(ctx)
	defer cn.Close()

	id := data.Get("id")
	name := data.Get("name")
	value, err := cn.SIsMember(ctx, "IDSet", id).Result()
	if err != nil {
		panic(err)
	}
	if value {
		fmt.Fprint(w, -1)
	} else {
		v, e := cn.SAdd(ctx, "IDSet", id).Result()
		if e != nil {
			panic(err)
		}
		_, e1 := cn.HSet(ctx, "IDNameHash", id, name).Result()
		if e1 != nil {
			panic(e1)
		}
		fmt.Fprint(w, v)
	}
}

func setNameHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	data := r.URL.Query()
	cn := rdb.Conn(ctx)
	defer cn.Close()

	id := data.Get("id")
	name := data.Get("name")
	_, err := cn.HSet(ctx, "IDNameHash", id, name).Result()
	if err != nil {
		panic(err)
	}
}

func getNameHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	data := r.URL.Query()
	cn := rdb.Conn(ctx)
	defer cn.Close()

	id := data.Get("id")
	name, err := cn.HGet(ctx, "IDNameHash", id).Result()
	if err != nil {
		panic(err)
	}
	fmt.Fprint(w, name)
}

func rankingHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	data := r.URL.Query()
	cn := rdb.Conn(ctx)
	defer cn.Close()

	if id := data.Get("id"); id == "" {
		value, err := cn.ZRevRangeWithScores(ctx, "rankingZset", 0, -1).Result()
		if err != nil {
			panic(err)
		}
		var IDs []string
		for _, content := range value {
			IDs = append(IDs, content.Member.(string))
		}
		names, e := cn.HMGet(ctx, "IDNameHash", IDs...).Result()
		if e != nil {
			panic(e)
		}
		for i, content := range value {
			fmt.Fprintf(w, "%d %s\n", int(content.Score), names[i])
		}
	} else {
		value, err := cn.ZRevRank(ctx, "rankingZset", id).Result()
		if err == redis.Nil {
			fmt.Fprint(w, -1)
		} else if err != nil {
			panic(err)
		} else {
			fmt.Fprint(w, value)
		}
	}
}

func setRankingHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	data := r.URL.Query()
	cn := rdb.Conn(ctx)
	defer cn.Close()

	id := data.Get("id")
	score := data.Get("score")
	s, _ := strconv.ParseFloat(score, 64)
	_, err := cn.ZAdd(ctx, "rankingZset", redis.Z{Score: s, Member: id}).Result()
	if err != nil {
		panic(err)
	}
}

func startHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	data := r.URL.Query()

	id := data.Get("id")
	client, err := rpc.Dial("tcp", "localhost:1200")
	if err != nil {
		panic(err)
	}

	var reply string
	err = client.Call("RoomCenterService.Start", id, &reply)
	defer client.Close()
	if err != nil {
		panic(err)
	}
	fmt.Fprint(w, reply)
}
