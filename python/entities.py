class Currency:
    def __init__(self, code:str, name:str, decimals:int = 2):
        self.Code = code
        self.Name = name
        self.Decimals = decimals

class Asset:
    def __init__(self, id:str, name:str):
        self.id = id
        self.name = name

class Provider:
    def __init__(self, id:str, name:str):
        self.Id = id
        self.Name = name