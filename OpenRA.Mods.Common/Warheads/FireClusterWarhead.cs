﻿#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;
using OpenRA.GameRules;

namespace OpenRA.Mods.Common.Warheads
{
	public class FireClusterWarhead : Warhead
	{
		[WeaponReference, FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string Weapon = null;

		[Desc("The range of the cells where the weapon should be fired.")]
		public readonly int Range = 0;

		public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers)
		{
			var map = firedBy.World.Map;
			var targetCells = map.FindTilesInCircle(map.CellContaining(target.CenterPosition), Range);
			var weapon = map.Rules.Weapons[Weapon.ToLowerInvariant()];

			foreach (var cell in targetCells) {

				var args = new ProjectileArgs
				{
					Weapon = weapon,
					Facing = Util.GetFacing(map.CenterOfCell(cell)-target.CenterPosition, 0),

					DamageModifiers = firedBy.TraitsImplementing<IFirepowerModifier>()
						.Select(a => a.GetFirepowerModifier()).ToArray(),

					InaccuracyModifiers = firedBy.TraitsImplementing<IInaccuracyModifier>()
						.Select(a => a.GetInaccuracyModifier()).ToArray(),

					RangeModifiers = firedBy.TraitsImplementing<IRangeModifier>()
						.Select(a => a.GetRangeModifier()).ToArray(),

					Source = target.CenterPosition,
					SourceActor = firedBy,
					PassiveTarget = map.CenterOfCell(cell),
					GuidedTarget = Target.FromCell(firedBy.World, cell)
				};

				if (args.Weapon.Projectile != null)
				{
					var projectile = args.Weapon.Projectile.Create(args);
					if (projectile != null)
						firedBy.World.Add(projectile);

					if (args.Weapon.Report != null && args.Weapon.Report.Any())
						Game.Sound.Play(args.Weapon.Report.Random(firedBy.World.SharedRandom), target.CenterPosition);
				}
			}

		}

		public bool IsValidImpact(WPos pos, Actor firedBy)
		{
			return true;
		}
	}
}

