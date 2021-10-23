namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities

type ICompanyLogic =
    abstract member Create:Company -> Result<Company>
    abstract member Update:Company -> Result<Company>

type CompanyValidation = Valid of Company | NotValid of string

type CompanyLogic(companyRepository:ICompanyRepository) =
    
    let assignNewId company:Company = {company with Id=Guid.NewGuid().ToString()}

    let normalize company:Company = {company with Name=company.Name.Trim()}

    let validate company = 
        if String.IsNullOrWhiteSpace(company.Name) then CompanyValidation.NotValid "Name cannot be empty."
        else CompanyValidation.Valid company

    interface ICompanyLogic with
        member this.Create(company:Company):Result<Company> =
            match normalize company |> validate with
            | NotValid msg -> Result<_>.NotValid msg
            | Valid newCompany ->
                match companyRepository.Exists(newCompany.Name) with 
                | true -> Result<_>.NotValid($"A company with name \"{newCompany.Name}\" already exists.")
                | _ -> companyRepository.Create(assignNewId newCompany)
                       Result<_>.Ok(newCompany)
            
        member this.Update (company:Company) =

            let checkNameExists company = 
                match companyRepository.GetByName company.Name with
                | Some c when c.Id <> company.Id -> NotValid $"A company with name \"{company.Name}\" already exists."
                | _ -> Valid company
         
            match normalize company |> validate with
            | Valid c -> match checkNameExists c with
                         | Valid c -> companyRepository.Update c; Result<_>.Ok c
                         | NotValid msg -> Result<_>.NotValid msg
            | NotValid msg -> Result<_>.NotValid msg