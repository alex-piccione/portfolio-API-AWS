module DU_spike


type GenericResult =
    | Ok
    | Error of string

type LoginResult =
    //| GenericResult.Ok
    | Ok
    | UserNotFound
    | WrongPassword
    //+ GeenricResult.Ok

type SaveResult =
    | Created of string
    | Updated
    //| GenericResult.Error


//let checkLogin something: LoginResult | GenericResult.Ok =
let checkLogin something: LoginResult =
    match something with
    | true -> LoginResult.Ok
    | _ -> WrongPassword


type Output<'TError> =
    | Ok
    | Error of 'TError

type LoginError = | UserDoesNotExist | WrongPassword

type LoginOutput = Output<LoginError>


let checkLogin2 something: LoginOutput =
    match something with
    | true -> Ok
    | _ -> Error WrongPassword


type Res = { isSuccess:bool}
type Fail = { error:string option}
type InvalidRequest = { reason:string option}
type Created<'a> = { item:'a}
//type Updated = {} end type
 

let success ():Res = { isSuccess=true }
let fail error: Res = { isSuccess=false }

let checkLogin3 something: Res =
    match something with
    | true -> success()
    | _ -> fail()


checkLogin true |> ignore