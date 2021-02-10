namespace Sandbox.FSharp

open System

module dataSets =
    let numberList = [ 1 .. 100 ]

module testFunctions =

    let rec factorial n =
        match n with
        | 0 | 1 -> 1
        | _ -> n * factorial(n-1)

    let results = dataSets.numberList |> List.map (fun n -> factorial n)
        
        
