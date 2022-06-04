namespace LinkNeuvo.Storage;

public interface IStorage
{
    void WriteResponses();
    void RecordResponse(StoredResponse clientResponse);
}