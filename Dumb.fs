module Dumb

open System

let handler (name: String) =
    if String.IsNullOrEmpty(name) then
        "Hello anon"
    else
        sprintf "Hello %s" name
