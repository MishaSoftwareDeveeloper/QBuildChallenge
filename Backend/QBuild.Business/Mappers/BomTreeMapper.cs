using QBuild.Models;

namespace QBuild.Business.Mappers;

public static class BomTreeMapper
{
    public static ComponentDto ToComponentDto(BomRecord record, Part? part)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new ComponentDto
        {
            ParentName = record.ParentName,
            ComponentName = record.ComponentName,
            Quantity = record.Quantity,
            PartNumber = part?.PartNumber,
            Title = part?.Title,
            Type = part?.Type,
            Item = part?.Item,
            Material = part?.Material
        };
    }
}
