using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Core.Minigame
{
    public class MinigameSessionRandomize
    {
        private readonly MinigamesDefinitionsSO _minigamesDefinitions;

        public MinigameSessionRandomize(MinigamesDefinitionsSO minigamesDefinitions)
        {
            _minigamesDefinitions = minigamesDefinitions;
        }

        public List<MinigameDefinition> CreateOrder()
        {
            var order = new List<MinigameDefinition>(_minigamesDefinitions.Minigames);
            for (int i = 0; i < order.Count; i++)
            {
                int randomIndex = Random.Range(i, order.Count);
                (order[i], order[randomIndex]) = (order[randomIndex], order[i]);
            }
            return order;
        }
    }
}
