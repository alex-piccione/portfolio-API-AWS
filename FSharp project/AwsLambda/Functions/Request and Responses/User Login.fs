module UserLogin

type LoginInput = { Email:string; Password:string }
//type LoginRequest = { Email:string; Username:string; Password:string }
//type LoginRequest = { Username:string; Password:string }


type LoginResult = { 
    IsSuccess: bool;
    Error: string 
    AuthToken: string 
}