namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities

type ICompanyLogic =
    abstract member Create:Company -> Result<Company, string>
    abstract member Update:Company -> Result<Company, string>
    abstract member Single:string -> Company option
    abstract member All:unit -> Company list
    abstract member Delete:string -> Result<unit,string>

type CompanyLogic(companyRepository:ICompanyRepository, fundRepository:IFundRepository) =
    
    let assignNewId company:Company = {company with Id=Guid.NewGuid().ToString()}

    let normalize company:Company = {company with Name=company.Name.Trim()}

    let validate company = 
        if String.IsNullOrWhiteSpace(company.Name) then Error "Name cannot be empty."
        else Ok company

    interface ICompanyLogic with
        member this.Create(company:Company) =
            match normalize company |> validate with
            | Error msg -> Error msg
            | Ok validCompany ->
                match companyRepository.Exists(validCompany.Name) with 
                | true -> Error $"A company with name \"{validCompany.Name}\" already exists."
                | _ -> 
                    let newCompany = assignNewId validCompany
                    companyRepository.Create(newCompany)
                    Ok newCompany
            
        member this.Update (company:Company) =
            match normalize company |> validate with
            | Error msg -> Error msg
            | Ok validCompany ->
                match companyRepository.GetByName company.Name with
                | Some c when c.Id <> company.Id -> Error $"Another company with name \"{company.Name}\" already exists."
                | _ -> companyRepository.Update validCompany; Ok validCompany

        member this.Single id = companyRepository.Single id

        member this.All () = List.ofSeq (companyRepository.All ())

        member this.Delete id =
            match (fundRepository.GetFundsOfCompany id).IsEmpty with
            | true -> companyRepository.Delete id; Ok ()
            | _ -> Error $"Company \"{id}\" is used in funds."