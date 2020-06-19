// Learn more about F# at http://fsharp.org

open System
open Swifter.Json

type Payment = Cache of decimal | CreditCard of int | Free
[<Measure>] type cm
type public Demo =
  { Id : int
    Name : string
    Age : int
    BodyHeight : int<cm> option
    Children : string list
    Payment : Payment
    WorkDone : Result<int,string> }
let value =
    { Id = 1
      Name = "Dogwei"
      Age = 42
      BodyHeight = Some 195<cm>
      Children = ["Andy"]
      Payment = Free
      WorkDone = Error "Not satisfied"
    }



let json = JsonFormatter.SerializeObject(value, JsonFormatterOptions.Indented)

Console.WriteLine json

let orig = JsonFormatter.DeserializeObject<Demo> json

Console.WriteLine orig