using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pacman
{
    public class Animator
    {
        public delegate IEnumerator<Image> GetAnimator();
        private GetAnimator GetAnimations;
        private IEnumerator<Image> IAnimator = null;
        public bool Condition = false;
        private Func<Image> GetDefault;
        public event EventHandler AnimationEnd;

        public Animator(GetAnimator getAnimations, Func<Image> getDefault)
        {
            GetAnimations = getAnimations;
            GetDefault = getDefault;
        }

        public Image Animate()
        {
            if (Condition)
            {
                IAnimator ??= GetAnimations();
                if (IAnimator.MoveNext())
                    return IAnimator.Current;
                else
                {
                    Condition = false;
                    IAnimator = null;
                    AnimationEnd?.Invoke(this, EventArgs.Empty);
                }
            }
            return GetDefault();
        }
    }
}