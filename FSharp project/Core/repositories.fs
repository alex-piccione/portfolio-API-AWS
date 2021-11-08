namespace Portfolio.Core

open System
open Portfolio.Core.Entities
open System.Collections.Generic

type CRUD<'T> =
    abstract member Create: 'T -> unit
    abstract member Single: string -> 'T option
    abstract member Update: 'T -> unit
    abstract member Delete: string -> unit

type All<'T> =
    abstract member All: unit -> IEnumerable<'T>

type List<'T> =
    abstract member List: skip:int -> take:int -> IEnumerable<'T>

type IUserRepository =
    inherit CRUD<User>
    inherit All<User>

type ICurrencyRepository =
    inherit CRUD<Currency>
    inherit All<Currency>

type ICompanyRepository =
    inherit CRUD<Company>
    inherit All<Company>
    abstract member Exists: name:string -> bool
    abstract member GetByName: name:string -> Company option

type ISessionRepository =
    abstract member Find: token:string -> Session option
    abstract member FindByEmail: email:string -> Session option
    abstract member Create: session:Session -> unit
    abstract member DeleteExpiredSessions: thresholdDate:DateTime -> unit

type IFundRepository = 
    //inherit CRUD<FundAtDate>
    //abstract member Save: fund:FundAtDate -> unit
    //abstract member GetFundsAtDate: date:DateTime -> FundAtDate list
    abstract member GetFundsToDate: date:DateTime -> FundAtDate list
    //abstract member InsertOrUpdateFundAtDate: record:FundAtDate -> unit
    abstract member FindFundAtDate: record:FundAtDate -> FundAtDate option
    abstract member UpdateFundAtDate: record:FundAtDate -> unit
    abstract member CreateFundAtDate: record:FundAtDate -> unit
