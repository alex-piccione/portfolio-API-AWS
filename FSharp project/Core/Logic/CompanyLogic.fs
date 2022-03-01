namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities

type ICompanyLogic =
    abstract member Create:Company -> Result<Company, string> // CreateResult
    abstract member Update:Company -> Result<Company, string> // UpdateResult
    abstract member Single:string -> Company option
    abstract member List:unit -> Company list
    abstract member Delete:string -> Result<unit,string>  // DeleteResult

type CompanyValidation = Valid of Company | NotValid of string

type CompanyLogic(companyRepository:ICompanyRepository) =
    
    let assignNewId company:Company = {company with Id=Guid.NewGuid().ToString()}

    let normalize company:Company = {company with Name=company.Name.Trim()}

    let validate company = 
        if String.IsNullOrWhiteSpace(company.Name) then CompanyValidation.NotValid "Name cannot be empty."
        else CompanyValidation.Valid company

    interface ICompanyLogic with
        member this.Create(company:Company) = //:CreateResult<Company> =
            match normalize company |> validate with
            | NotValid msg -> Error msg
            | Valid validCompany ->
                match companyRepository.Exists(validCompany.Name) with 
                | true -> Error $"A company with name \"{validCompany.Name}\" already exists."
                | _ -> 
                    let newCompany = assignNewId validCompany
                    companyRepository.Create(newCompany)
                    Ok newCompany
            
        member this.Update (company:Company) =
            let checkNameExists company = 
                match companyRepository.GetByName company.Name with
                | Some c when c.Id <> company.Id -> NotValid $"A company with name \"{company.Name}\" already exists."
                | _ -> Valid company
         
            match normalize company |> validate with
            | NotValid msg -> Error msg
            | Valid validCompany -> 
                match checkNameExists validCompany with
                | Valid c -> companyRepository.Update c; Ok c
                | NotValid msg -> Error msg

        member this.Single id = companyRepository.Single id

        member this.List () = List.ofSeq (companyRepository.All ())

        member this.Delete id =
            companyRepository.Delete id;
            Ok ()
            //DeleteResult.Deleted