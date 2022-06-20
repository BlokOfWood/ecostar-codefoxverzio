public interface AIAction
{
    void initialize(Entity AIStats);
    void execute(AIController controller, float deltaTime);
}