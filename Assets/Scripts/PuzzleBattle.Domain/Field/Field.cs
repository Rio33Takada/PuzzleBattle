using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Domain.Field
{
    public sealed class Field
    {
        private readonly FieldObject[] objects;

        public Field(GridShape range, IEnumerable<FieldObject> objects = null)
        {
            Range = range ?? throw new ArgumentNullException(nameof(range));
            this.objects = objects == null ? Array.Empty<FieldObject>() : objects.ToArray();
            GridObjectCollectionValidator.Validate(Range, this.objects);
        }

        public GridShape Range { get; }

        public IReadOnlyCollection<FieldObject> Objects => objects;
    }
}
