namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities

type ICompanyLogic =
    abstract member Create:Company -> Result<Company>
    abstract member Update:Company -> Result<Company>

type CompanyLogic(companyRepository:ICompanyRepository) =
    
    let assignNewId company = {company with Id=Guid.NewGuid().ToString()}

    let normalize company:Company = {company with Name=company.Name.Trim()}

    let validate company = 
        if String.IsNullOrWhiteSpace(company.Name) then failwith "\"Name\" cannot be empty."
        company

    interface ICompanyLogic with
        member this.Create(company:Company):Result<Company> =
            let newCompany = normalize company |> validate |> assignNewId

            match companyRepository.Exists(newCompany.Name) with 
            | true -> Result<_>.NotValid($"A company with name \"{newCompany.Name}\" already exists.")
            | _ -> companyRepository.Create(newCompany)
                   Result<_>.Ok(newCompany)
            
        member this.Update (company:Company) =
            let updateCompany = normalize company

            match companyRepository.GetByName updateCompany.Name with
            | Some _ -> Result<_>.NotValid($"A company with name \"{updateCompany.Name}\" already exists.")
            | _ -> companyRepository.Update updateCompany
                   Result<_>.Ok(updateCompany)