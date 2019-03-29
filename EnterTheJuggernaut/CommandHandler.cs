using Smod2.API;
using Smod2.Commands;

using System.Linq;

namespace EnterTheJuggernaut
{
	public class CommandHandler : ICommandHandler
	{
		private EnterTheJuggernaut plugin;

		public CommandHandler(EnterTheJuggernaut plugin)
		{
			this.plugin = plugin;
		}
		public string GetCommandDescription()
		{
			return "Triggers the EnterTheJuggernaut event round";
		}
		public string GetUsage()
		{
			return "juggernaut";
		}
		public string[] OnCall(ICommandSender sender, string[] args)
		{
			bool valid = sender is Server;
			Player player = null;
			if (!valid)
			{
				player = sender as Player;
				if (player != null)
				{
					valid = plugin.ETJranks.Contains(player.GetRankName());
				}
			}

			if (valid)
			{
				plugin.Enabled = true;
				return new[] { "EnterTheJuggernaut has been enabled for the next round" };
			}

			return new[]
			{
				"EnterTheJuggernaut is already enabled."
			};
		}
	}
}
