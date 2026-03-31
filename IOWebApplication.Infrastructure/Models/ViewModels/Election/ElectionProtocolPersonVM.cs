// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Election
{
    public class ElectionProtocolPersonVM
    {
        public int Id { get; set; }
        public string LawUnitFullName { get; set; }
         public int CourtId { get; set; }
        public string CourtName { get; set; }
        public int ElectionPersonStateId { get; set; }
           
         //--статус в протокола
         public string DescriptionState { get; set; }
        //--статус в протокола
         //---отвода
         public int? ElectionPersonDismissalTypeId { get; set; }
        public string ElectionPersonDismissalTypeName { get; set; }
         public string DescriptionRemoved { get; set; }
    }
}
