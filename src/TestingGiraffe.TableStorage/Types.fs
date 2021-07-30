namespace TableStorage

module Types = 
    open FSharp.Azure.Storage.Table

    type Game = {
        [<PartitionKey>] Developer: string
        [<RowKey>] Id: string
        Name: string
        HasMultiplayer: bool }