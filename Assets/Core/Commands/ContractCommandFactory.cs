using UnityEngine;
using Zenject;
using Core.Interfaces;

namespace Core.Commands
{
    /// <summary>
    /// Factory for creating contract-related commands.
    /// Follows factory pattern for centralized object creation.
    /// </summary>
    public class ContractCommandFactory : IFactory<ContractCommandFactory.CreateCommandParams, ITabletCommand>
    {
        private readonly IContractsService contractsService;
        private readonly ICrewService crewService;

        public ContractCommandFactory(
            IContractsService contractsService,
            ICrewService crewService)
        {
            this.contractsService = contractsService;
            this.crewService = crewService;
        }

        public ITabletCommand Create(CreateCommandParams parameters)
        {
            switch (parameters.CommandType)
            {
                case ContractCommandType.Accept:
                    return new AcceptContractCommand(
                        contractsService,
                        crewService,
                        parameters.ContractId,
                        parameters.PlayerQualifications);

                case ContractCommandType.Complete:
                    return new CompleteContractCommand(
                        contractsService,
                        crewService,
                        parameters.ContractId,
                        parameters.PerformanceScore);

                default:
                    throw new System.ArgumentException($"Unknown contract command type: {parameters.CommandType}");
            }
        }

        public struct CreateCommandParams
        {
            public ContractCommandType CommandType;
            public string ContractId;
            public string[] PlayerQualifications;
            public int PerformanceScore;
        }

        public enum ContractCommandType
        {
            Accept,
            Complete
        }
    }

    /// <summary>
    /// Factory for creating crew-related commands.
    /// </summary>
    public class CrewCommandFactory : IFactory<CrewCommandFactory.CreateCommandParams, ITabletCommand>
    {
        private readonly ICrewService crewService;

        public CrewCommandFactory(ICrewService crewService)
        {
            this.crewService = crewService;
        }

        public ITabletCommand Create(CreateCommandParams parameters)
        {
            switch (parameters.CommandType)
            {
                case CrewCommandType.Hire:
                    return new HireCrewCommand(
                        crewService,
                        parameters.CrewId,
                        parameters.Role);

                case CrewCommandType.Fire:
                    return new FireCrewCommand(
                        crewService,
                        parameters.CrewId);

                case CrewCommandType.ChangeRole:
                    return new ChangeCrewRoleCommand(
                        crewService,
                        parameters.CrewId,
                        parameters.Role);

                default:
                    throw new System.ArgumentException($"Unknown crew command type: {parameters.CommandType}");
            }
        }

        public struct CreateCommandParams
        {
            public CrewCommandType CommandType;
            public string CrewId;
            public Core.Data.CrewRole Role;
        }

        public enum CrewCommandType
        {
            Hire,
            Fire,
            ChangeRole
        }
    }

    /// <summary>
    /// Factory for creating intel-related commands.
    /// </summary>
    public class IntelCommandFactory : IFactory<IntelCommandFactory.CreateCommandParams, ITabletCommand>
    {
        private readonly IIntelService intelService;
        private readonly ICrewService crewService;

        public IntelCommandFactory(
            IIntelService intelService,
            ICrewService crewService)
        {
            this.intelService = intelService;
            this.crewService = crewService;
        }

        public ITabletCommand Create(CreateCommandParams parameters)
        {
            switch (parameters.CommandType)
            {
                case IntelCommandType.Verify:
                    return new VerifyIntelCommand(
                        intelService,
                        crewService,
                        parameters.IntelId,
                        parameters.CrewId);

                case IntelCommandType.CrossReference:
                    return new CrossReferenceIntelCommand(
                        intelService,
                        parameters.IntelId);

                default:
                    throw new System.ArgumentException($"Unknown intel command type: {parameters.CommandType}");
            }
        }

        public struct CreateCommandParams
        {
            public IntelCommandType CommandType;
            public string IntelId;
            public string CrewId;
        }

        public enum IntelCommandType
        {
            Verify,
            CrossReference
        }
    }

    /// <summary>
    /// Factory for creating upgrade-related commands.
    /// </summary>
    public class UpgradeCommandFactory : IFactory<UpgradeCommandFactory.CreateCommandParams, ITabletCommand>
    {
        private readonly ICrewService crewService;
        private readonly IUpgradeService upgradeService;

        public UpgradeCommandFactory(
            ICrewService crewService,
            IUpgradeService upgradeService)
        {
            this.crewService = crewService;
            this.upgradeService = upgradeService;
        }

        public ITabletCommand Create(CreateCommandParams parameters)
        {
            switch (parameters.CommandType)
            {
                case UpgradeCommandType.Purchase:
                    return new PurchaseUpgradeCommand(
                        crewService,
                        upgradeService,
                        parameters.UpgradeId);

                default:
                    throw new System.ArgumentException($"Unknown upgrade command type: {parameters.CommandType}");
            }
        }

        public struct CreateCommandParams
        {
            public UpgradeCommandType CommandType;
            public string UpgradeId;
        }

        public enum UpgradeCommandType
        {
            Purchase
        }
    }
}