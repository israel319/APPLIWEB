namespace AppPlusPlus.Domain.Enums.GRH;

/// <summary>
/// Statuts possibles d'un employé
/// </summary>
public enum EmployeeStatusEnum
{
    /// <summary>Employé actif</summary>
    Active = 1,
    
    /// <summary>Employé inactif</summary>
    Inactive = 2,
    
    /// <summary>Employé en congé</summary>
    OnLeave = 3,
    
    /// <summary>Employé suspendu</summary>
    Suspended = 4,
    
    /// <summary>Employé résilié</summary>
    Terminated = 5,
    
    /// <summary>Employé en congé maternité</summary>
    OnMaternity = 6
}
