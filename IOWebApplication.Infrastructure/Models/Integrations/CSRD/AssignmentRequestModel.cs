// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;


namespace IOWebApplication.Infrastructure.Models.Integrations.CSRD
{
  public class AssignmentRequestModel
  {


    public int Case_ID { get; set; }

    public int Judge_ID { get; set; }



    public DateTime AssignmentDate { get; set; }

    public int TypeOfAssignment { get; set; }


    public byte[] Protocol { get; set; }

    public byte[] ProtocolSignature { get; set; }


    public string CaseNumber { get; set; }


    public DateTime CaseFormationDate { get; set; }



    public string Name { get; set; }


    public string Family { get; set; }


    public int CaseCode { get; set; }


    public int CourtNumber { get; set; }

    public int CaseYear { get; set; }



  }
}