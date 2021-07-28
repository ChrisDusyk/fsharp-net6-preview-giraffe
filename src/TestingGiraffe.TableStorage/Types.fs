namespace TestingGiraffe.TableStorage

module Types =
    open FSharp.Azure.Storage.Table

    type Game = {
        [<PartitionKey>] Developer: string
        [<RowKey>] Id: string
        Name: string
        HasMultiplayer: bool }

module Result =
    let traverseResult f list =
        let (>>=) x f = Result.bind f x
        let retn = Result.Ok

        let cons head tail = head :: tail

        let initState = retn []
        let folder head tail =
            f head >>= (fun h ->
                tail >>= (fun t ->
                    retn (cons h t) ))
        Seq.foldBack folder list initState