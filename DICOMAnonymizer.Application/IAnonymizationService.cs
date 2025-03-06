namespace DICOMAnonymizer.Application
{
    public interface IAnonymizationService
    {
        void AnonymizeFolder(string inputFolderPath, string outputFolderPath);
    }
}