using System.Runtime.Serialization;

namespace SimpleReader {
    [DataContract]
    public class Settings {
        [DataMember]
        private bool fullscreen;
        [DataMember]
        private bool animation;
        [DataMember]
        private bool sorting;

        public bool IsFullscreen {
            get { return fullscreen; }
        }
        public bool IsAnimationEnabled {
            get { return animation; }
        }
        public bool IsSortingEnabled {
            get { return sorting; }
        }

        public Settings() {
            fullscreen = false;
            animation = true;
            sorting = false;
        }

        public void SetFullscreenMode(bool value) { fullscreen = value; }
        public void EnableAnimation() { animation = true; }
        public void DisableAnimation() { animation = false; }
        public void EnableSorting() { sorting = true; }
        public void DisableSorting() { sorting = false; }
    }
}
