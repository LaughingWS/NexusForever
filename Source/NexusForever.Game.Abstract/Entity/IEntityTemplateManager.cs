namespace NexusForever.Game.Abstract.Entity
{
    public interface IEntityTemplateManager
    {
        /// <summary>
        /// Initialise <see cref="IEntityTemplate"/>'s from the database.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Return <see cref="IEntityTemplate"/> with the supplied id.
        /// </summary>
        IEntityTemplate GetEntityTemplate(uint id);
    }
}
