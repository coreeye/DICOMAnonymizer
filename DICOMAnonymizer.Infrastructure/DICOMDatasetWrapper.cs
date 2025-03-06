using DICOMAnonymizer.Core;
using FellowOakDicom;

namespace DICOMAnonymizer.Infrastructure;

public class DICOMDatasetWrapper(DicomDataset dataset) : IDICOMDataset
{
    private readonly DicomDataset _dataset = dataset;
    public DicomDataset DicomDataset => _dataset;

    public string GetString(DICOMCoreTag tag)
    {
        var dicomTag = new DicomTag((ushort)Convert.ToInt32(tag.Group, 16), (ushort)Convert.ToInt32(tag.Element, 16));
        return _dataset.GetString(dicomTag);
    }

    public void AddOrUpdate(DICOMCoreTag tag, string? value)
    {
        var dicomTag = new DicomTag((ushort)Convert.ToInt32(tag.Group, 16), (ushort)Convert.ToInt32(tag.Element, 16));
        _dataset.AddOrUpdate(dicomTag, value);
    }

    public bool Contains(DICOMCoreTag tag)
    {
        var dicomTag = new DicomTag((ushort)Convert.ToInt32(tag.Group, 16), (ushort)Convert.ToInt32(tag.Element, 16));
        return _dataset.Contains(dicomTag);
    }

    public void Remove(DICOMCoreTag tag)
    {
        var dicomTag = new DicomTag((ushort)Convert.ToInt32(tag.Group, 16), (ushort)Convert.ToInt32(tag.Element, 16));
        _dataset.Remove(dicomTag);
    }

    public IEnumerator<DICOMCoreTag> GetEnumerator()
    {
        foreach (var item in _dataset)
        {
            yield return new DICOMCoreTag(
                item.Tag.Group.ToString("X4"),
                item.Tag.Element.ToString("X4")
            );
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}