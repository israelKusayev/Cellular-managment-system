﻿using Common.DataConfig;
using Common.Enums;
using Common.Exeptions;
using Common.Interfaces.ServerManagersInterfaces;
using Common.Logger;
using Common.Models;
using Common.ModelsDTO;
using Common.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Managers
{
    public class SimulatorManager : ISimulatorManager
    {
        private IUnitOfWork _unitOfWork;
        private LoggerManager _logger;
        private Random _durationRand;
        private Random _destinationRand;

        //ctor
        public SimulatorManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = new LoggerManager(new FileLogger(), "simulatorManager.txt");
            _durationRand = new Random();
            _destinationRand = new Random();
        }

        /// <summary>
        /// Simulate calls or sms to a specific line
        /// </summary>
        /// <param name="simulateDTO">Simulate detalis</param>
        /// <returns>True if succeeded otherwise false</returns>
        public bool SimulateCallsOrSms(SimulateDTO simulateDTO)
        {
            Customer selectedCustomer;
            Line selectedLine;

            try
            {
                selectedCustomer = _unitOfWork.Customer.GetActiveCustomerWithLines(simulateDTO.IdentityCard);
                selectedLine = _unitOfWork.Line.GetLineWithPackageAndFriends(simulateDTO.LineId);

                selectedCustomer.CallsToCenter += simulateDTO.CallToCenter;
                _unitOfWork.Complete();
            }
            catch (Exception e)
            {
                _logger.Log($"{Messages.messageFor[MessageType.GeneralDbFaild]} Execption details: {e.Message}");
                throw new FaildToConnectDbExeption(Messages.messageFor[MessageType.GeneralDbFaild]);
            }

            List<string> destinationNumbers = GetDestinationNumbers(simulateDTO, selectedCustomer, selectedLine);

            if (destinationNumbers == null)
            {
                throw new NotFoundException("destination number");
            }

            if (simulateDTO.IsSms)
            {
                for (int i = 0; i < simulateDTO.NumberOfCallsOrSms; i++)
                {
                    string destination = destinationNumbers[_destinationRand.Next(destinationNumbers.Count)];
                    AddSms(simulateDTO.LineId, destination);
                }
                return true;
            }
            else
            {
                for (int i = 0; i < simulateDTO.NumberOfCallsOrSms; i++)
                {
                    string destination = destinationNumbers[_destinationRand.Next(destinationNumbers.Count)];

                    int douration = _durationRand.Next(simulateDTO.MinDuration, simulateDTO.MaxDuration + 1);
                    AddCall(simulateDTO.LineId, destination, douration);
                }
                return true;
            }
        }

        /// <summary>
        /// Get the destination numbers according to the selected group of send to
        /// </summary>
        /// <param name="simulateDTO">Simulate details</param>
        /// <param name="customer">Customer details</param>
        /// <param name="line">Line details</param>
        /// <returns>List of destination number if succeeded otherwise null</returns>
        private List<string> GetDestinationNumbers(SimulateDTO simulateDTO, Customer customer, Line line)
        {
            List<string> numbers = new List<string>();

            switch (simulateDTO.SendTo)
            {
                case SimulateSendTo.Friends:
                    {
                        if (line.Package.Friends.FirstNumber != null)
                            numbers.Add(line.Package.Friends.FirstNumber);
                        if (line.Package.Friends.SecondNumber != null)
                            numbers.Add(line.Package.Friends.SecondNumber);
                        if (line.Package.Friends.ThirdNumber != null)
                            numbers.Add(line.Package.Friends.ThirdNumber);
                        return numbers;
                    }

                case SimulateSendTo.Family:
                    {
                        return customer.Lines.Where((l) => l.LineNumber != line.LineNumber).Select((x) => x.LineNumber).ToList();
                    }

                case SimulateSendTo.General: //Get destination numbers without family and friends
                    {
                        simulateDTO.SendTo = SimulateSendTo.Family;
                        List<string> family = GetDestinationNumbers(simulateDTO, customer, line);
                        simulateDTO.SendTo = SimulateSendTo.Friends;
                        List<string> friends = GetDestinationNumbers(simulateDTO, customer, line);
                        simulateDTO.SendTo = SimulateSendTo.All;
                        List<string> all = GetDestinationNumbers(simulateDTO, customer, line);

                        return all.Except(family).Except(friends).ToList();
                    }

                case SimulateSendTo.All:
                    {
                        try
                        {
                            IEnumerable<Line> lines;
                            lines = _unitOfWork.Line.GetAll();
                            var linesNumber = lines.Where(l => l.LineNumber != line.LineNumber).Select(l => l.LineNumber).ToList();
                            return linesNumber;
                        }
                        catch (Exception e)
                        {
                            _logger.Log($"{Messages.messageFor[MessageType.GeneralDbFaild]} Execption details: {e.Message}");
                            throw new FaildToConnectDbExeption(Messages.messageFor[MessageType.GeneralDbFaild]);
                        }
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create new Sms 
        /// </summary>
        /// <param name="from">sender</param>
        /// <param name="to">reciver</param>
        private void AddSms(int from, string to)
        {
            try
            {
                Sms sms = new Sms
                {
                    LineId = from,
                    DestinationNumber = to,
                    DataOfMessage = DateTime.Now
                };
                _unitOfWork.Sms.Add(sms);
                _unitOfWork.Complete();
            }
            catch (Exception e)
            {
                _logger.Log($"{Messages.messageFor[MessageType.GeneralDbFaild]} Execption details: {e.Message}");
                throw new FaildToConnectDbExeption(Messages.messageFor[MessageType.GeneralDbFaild]);
            }
        }

        /// <summary>
        /// Create new call
        /// </summary>
        /// <param name="from">sender</param>
        /// <param name="to">Reciver</param>
        /// <param name="duration">Call duration in seconds</param>
        private void AddCall(int from, string to, int duration)
        {
            try
            {
                Call call = new Call
                {
                    LineId = from,
                    DestinationNumber = to,
                    Duration = duration,
                    DateOfCall = DateTime.Now
                };
                _unitOfWork.Call.Add(call);
                _unitOfWork.Complete();
            }
            catch (Exception e)
            {
                _logger.Log($"{Messages.messageFor[MessageType.GeneralDbFaild]} Execption details: {e.Message}");
                throw new FaildToConnectDbExeption(Messages.messageFor[MessageType.GeneralDbFaild]);
            }
        }
    }
}