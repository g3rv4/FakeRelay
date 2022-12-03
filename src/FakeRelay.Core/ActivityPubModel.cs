namespace FakeRelay.Core;

public class ActivityPubModel
{
    public string Id { get; set; }
    public string Actor { get; set; }
    private Uri? _actorUrl;
    public Uri ActorUrl => _actorUrl ??= new Uri(Actor);
    public string Type { get; set; }
}