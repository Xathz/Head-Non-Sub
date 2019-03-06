namespace HeadNonSub.Statistics {

    public interface IDirty {

        bool IsDirty { get; }

        /// <summary>
        /// Sets <see cref="IsDirty"/> to <see langword="false" />.
        /// </summary>
        void MarkClean();

    }

}
