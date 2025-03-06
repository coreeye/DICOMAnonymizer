namespace DICOMAnonymizer.Core;

public interface IDICOMDataset : IEnumerable<DICOMCoreTag>
{
    string GetString(DICOMCoreTag tag);
    void AddOrUpdate(DICOMCoreTag tag, string? value);
    bool Contains(DICOMCoreTag tag);
    void Remove(DICOMCoreTag tag);
}