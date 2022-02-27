type ResultA = Result<string, int>
let a : ResultA = Ok "A"
match box a with
| :? Result<string, obj> -> printfn "match ResultA"
| _ -> printfn "no match"