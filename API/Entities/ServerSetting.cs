using System.ComponentModel.DataAnnotations;
using API.Entities.Enums;
using API.Entities.Interfaces;

namespace API.Entities;

public class ServerSetting: IHasConcurrencyToken
{
    [Key]
    public required ServerSettingKey Key { get; set; }
    
    public required string Value { get; set; }

    public uint RowVersion { get; private set; }

    public void OnSavingChanges()
    {
        RowVersion++;
    }
}