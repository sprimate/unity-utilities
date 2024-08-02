namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class SizedFoldoutGroupAttribute : PropertyGroupAttribute
    {
        private bool expanded; 

        public bool Expanded
        {
            get { return this.expanded; }
            set
            {
                this.expanded = value;
                this.HasDefinedExpanded = true;
            }
        }

        public bool HasDefinedExpanded { get; private set; }
        public bool HasDefinedColor = false;

        public float R, G, B, A;

        public SizedFoldoutGroupAttribute(string groupName, float order = 0)
            : base(groupName, order)
        {
        }

        public SizedFoldoutGroupAttribute(string groupName, float r, float g, float b, float a = 1f, float order = 0)
            : base(groupName, order)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
            HasDefinedColor = true;
        }

        public SizedFoldoutGroupAttribute(string groupName, bool expanded, float order = 0)
            : base(groupName, order)
        {
            this.expanded = expanded;
            this.HasDefinedExpanded = true;
        }

        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var attr = other as SizedFoldoutGroupAttribute;
            if (attr.HasDefinedExpanded)
            {
                this.HasDefinedExpanded = true;
                this.Expanded = attr.Expanded;
            }

            if (this.HasDefinedExpanded)
            {
                attr.HasDefinedExpanded = true;
                attr.Expanded = this.Expanded;
            }

            this.R = Math.Max(attr.R, this.R);
            this.G = Math.Max(attr.G, this.G);
            this.B = Math.Max(attr.B, this.B);
            this.A = Math.Max(attr.A, this.A);
        }
    }
}