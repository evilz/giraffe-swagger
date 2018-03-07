namespace giraffe_swagger.Models

open System
[<CLIMutable>]
type Message =
    {
        Text : string
    }

type Child = {
    prop1: string
    prop2: int
}

type Parent = {
    date: DateTime
    child: Child
}