using System;
using System.Collections.Generic;
using System.Linq;
using Nanover.Core.Science;
using Nanover.Visualisation.Properties;
using Nanover.Visualisation.Properties.Collections;
using Nanover.Visualisation.Property;
using UnityEngine;

namespace Nanover.Visualisation.Node.Protein
{
    /// <summary>
    /// Calculates sequences of subsequent residues in entities which are standard
    /// amino acids, hence forming polypeptide chains.
    /// </summary>
    [Serializable]
    public class PolypeptideSequenceNode : GenericOutputNode
    {
        public IProperty<string[]> AtomNames => atomNames;
        
        /// <summary>
        /// The name of each atom.
        /// </summary>
        [SerializeField]
        private StringArrayProperty atomNames = new StringArrayProperty();

        public IProperty<int[]> AtomResidues => atomResidues;
        
        /// <summary>
        /// The residue index for each atom.
        /// </summary>
        [SerializeField]
        private IntArrayProperty atomResidues = new IntArrayProperty();

        public IProperty<string[]> ResidueNames => residueNames;
        
        /// <summary>
        /// The name of each residue.
        /// </summary>
        [SerializeField]
        private StringArrayProperty residueNames = new StringArrayProperty();

        public IProperty<int[]> ResidueEntities => residueEntities;
        
        /// <summary>
        /// The entity (chain) index for each residue.
        /// </summary>
        [SerializeField]
        private IntArrayProperty residueEntities = new IntArrayProperty();

        public IProperty<IReadOnlyList<int>[]> ResidueSequences => residueSequences;

        /// <summary>
        /// A set of sequences of residue indices that form polypeptide chains.
        /// </summary>
        private readonly SelectionArrayProperty residueSequences = new SelectionArrayProperty();

        /// <summary>
        /// A set of alpha carbon atom indices that form polypeptide chains.
        /// </summary>
        private readonly IntArrayProperty alphaCarbonSequences = new IntArrayProperty();
        
        /// <summary>
        /// The lengths of each sequence.
        /// </summary>
        private readonly IntArrayProperty sequenceLengths = new IntArrayProperty();

        /// <inheritdoc cref="IsInputValid" />
        protected override bool IsInputValid => atomNames.HasNonNullValue()
                                             && atomResidues.HasNonNullValue()
                                             && residueNames.HasNonNullValue()
                                             && residueEntities.HasNonNullValue();

        /// <inheritdoc cref="IsInputDirty" />
        protected override bool IsInputDirty => atomNames.IsDirty
                                             || atomResidues.IsDirty
                                             || residueNames.IsDirty
                                             || residueEntities.IsDirty;

        /// <inheritdoc cref="ClearDirty" />
        protected override void ClearDirty()
        {
            atomNames.IsDirty = false;
            atomResidues.IsDirty = false;
            residueNames.IsDirty = false;
            residueEntities.IsDirty = false;
        }

        /// <inheritdoc cref="UpdateOutput" />
        protected override void UpdateOutput()
        {
            var (resSequences, alphaSequences) = CalculateSequences(residueNames.Value,
                                                                    atomNames.Value,
                                                                    atomResidues.Value,
                                                                    residueEntities.Value);
            residueSequences.Value = resSequences;
            alphaCarbonSequences.Value = alphaSequences.SelectMany(i => i).ToArray();
            sequenceLengths.Value = alphaSequences.Select(i => i.Count).ToArray();
        }

        /// <summary>
        /// Calculate sequences of polypeptides
        /// </summary>
        private static (IReadOnlyList<int>[] residueSequences, IReadOnlyList<int>[] alphaCarbonSequences)
            CalculateSequences(string[] residueNames,
                               string[] atomNames,
                               int[] atomResidues,
                               int[] residueEntities)
        {
            var residueSequences = new List<IReadOnlyList<int>>();
            var alphaCarbonSequences = new List<IReadOnlyList<int>>();
            var currentResidues = new List<int>();
            var currentAlphaCarbons = new List<int>();
            var currentEntity = -1;

            for (var atom = 0; atom < atomNames.Length; atom++)
            {
                if (!string.Equals(atomNames[atom], "ca",
                                   StringComparison.InvariantCultureIgnoreCase))
                    continue;
                var residue = atomResidues[atom];
                //if (!AminoAcid.IsStandardAminoAcid(residueNames[residue]))
                //    continue;
                var entity = residueEntities[residue];
                if (currentEntity != entity && currentResidues.Any())
                {
                    residueSequences.Add(currentResidues);
                    alphaCarbonSequences.Add(currentAlphaCarbons);
                    currentResidues = new List<int>();
                    currentAlphaCarbons = new List<int>();
                }

                currentEntity = entity;
                currentResidues.Add(residue);
                currentAlphaCarbons.Add(atom);
            }

            if (currentResidues.Any())
            {
                residueSequences.Add(currentResidues);
                alphaCarbonSequences.Add(currentAlphaCarbons);
            }

            return (residueSequences.ToArray(), alphaCarbonSequences.ToArray());
        }

        /// <inheritdoc cref="ClearOutput" />
        protected override void ClearOutput()
        {
            residueSequences.UndefineValue();
            alphaCarbonSequences.UndefineValue();
        }
    }
}