var exitFile = [   //出境
    {
        type:'1',
        name:'企业法人营业执照(复印件)',
        isneed:true,
        single:false
    },
    {
        type:'8',
        name:'旅行社业务经营许可证正本(复印件)',
        isneed:true,
        single:false
    },
    {
        type:'9',
        name:'旅行社业务经营许可证副本(复印件)',
        isneed:true,
        single:false
    },
    {
        type:'39',
        name:'增存质量保证金承诺书',
        isneed:true,
        single:false
    },
    {
        type:'38',
        name:'连续两年未因侵害旅游者合法权益受到行政机关罚款以上处罚的承诺书',
        isneed:true,
        single:false
    }
];//出境

//旅行社资质附件
var agencyFileList = [
    {

        type:'1',
        name:'营业执照(原件)',
        isneed:true,
        single:false
    },
    {
        type:'2',
        name:'营业场所证明材料',
        isneed:true,
        single:false
    },
    {
        type:'4',
        name:'法定代表人履历',
        isneed:true,
        single:false
    },
    {
        type:'5',
        name:'企业章程',
        isneed:true,
        single:false
    },
    {
        type:'6',
        name:'法定代表人身份证正面',
        isneed:true,
        single:false
    },
    {
        type:'7',
        name:'法定代表人身份证反面',
        isneed:true,
        single:false
    },
    {
        type:'8',
        name:'旅行社业务经营许可证正本',
        isneed:true,
        single:false
    },
    {
        type:'9',
        name:'旅行社业务经营许可证副本',
        isneed:true,
        single:false
    },
    {
        type:'53',
        name:'办公设备证明材料',
        isneed:true,
        single:false
    },
    {
        type:'54',
        name:'其他相关材料证明',
        isneed:false,
        single:false
    }
];


var changeBranchFileList =[
];


var stopAgencyFileList =[
    {
        type:'8',
        name:'旅行社经营业务许可证原件正本',
        isneed:true,
        single:false
    },
    {
        type:'9',
        name:'旅行社经营业务许可证原件副本',
        isneed:true,
        single:false
    },
    {
        type:'6',
        name:'法人身份证正面',
        isneed:true,
        single:false
    },
    {
        type:'7',
        name:'法人身份证反面',
        isneed:true,
        single:false
    },
    {
        type:'44',
        name:'工商部门准予注销通知书',
        isneed:true,
        single:false
    },
    {
        type:'49',
        name:'其他相关材料附件(非必填)',
        isneed:false,
        single:false
    }

]//注销旅行社

var stopExitFileList =[
    {
        type:'8',
        name:'旅行社经营业务许可证原件正本',
        isneed:true,
        single:false
    },
    {
        type:'9',
        name:'旅行社经营业务许可证原件副本',
        isneed:true,
        single:false
    },
    {
        type:'52',
        name:'其他相关材料附件(非必填)',
        isneed:false,
        single:false
    }

]//注销出境

var branchFileList =[
    {
        type:'29',
        name:'分社经理身份证正面',
        isneed:true,
        single:false
    },
    {
        type:'30',
        name:'分社经理身份证反面',
        isneed:true,
        single:false
    },
    {
        type:'31',
        name:'分社经理履历表',
        isneed:true,
        single:false
    },
    {
        type:'32',
        name:'分社营业执照',
        isneed:true,
        single:false
    },
    //{
    //    type:'33',
    //    name:'分社租房协议或合同(多份)',
    //    isneed:false,
    //    single:false
    //},
    //{
    //    type:'34',
    //    name:'分社员工劳动合同(多份)',
    //    isneed:false,
    //    single:false
    //},
    {
        type:'43',
        name:'其他相关材料附件(非必填)',
        isneed:false,
        single:false
    }

]//申请备案/变更分社
var guaranteeFileList=[
    {
        type:'23',
        name:'旅游服务质量保证金存款协议书',
        isneed:true,
        single:false
    },
    {
        type:'42',
        name:'存款单号扫描件',
        isneed:true,
        single:false
    }
]//申请分社质保金
var bankFileList = [
    {
        type:'47',
        name:'旅游服务质量保证金银行担保承诺书',
        isneed:true,
        single:false
    }]//申请分社银行担保

var websiteFileList =[
    {
        type:'29',
        name:'网点经理身份证正面',
        isneed:true,
        single:false
    },
    {
        type:'30',
        name:'网点经理身份证反面',
        isneed:true,
        single:false
    },
    {
        type:'31',
        name:'网点经理履历表',
        isneed:true,
        single:false
    },
    {
        type:'32',
        name:'网点营业执照',
        isneed:true,
        single:false
    },
    /* {
     type:'33',
     name:'网点租房协议或合同(多份)',
     isneed:false,
     single:false
     },
     {
     type:'34',
     name:'网点员工劳动合同(多份)',
     isneed:false,
     single:false
     },*/
    {
        type:'43',
        name:'其他相关材料附件(非必填)',
        isneed:false,
        single:false
    }
]//申请备案/变更网点

var stopBranchFileList=[
    {
        type:'45',
        name:'工商行政管理部门核准注销登记的有效文件',
        isneed:true,
        single:false
    },
    {
        type:'46',
        name:'地(国)税局注销文件、组织机构代码注销文件',
        isneed:true,
        single:false
    },
    {
        type:'48',
        name:'其他相关材料附件(非必填)',
        isneed:false,
        single:false
    }
]//申请注销分社

var stopWebsiteFileList=[
    {
        type:'45',
        name:'工商行政管理部门核准注销登记的有效文件',
        isneed:true,
        single:false
    },
    {
        type:'46',
        name:'地(国)税局注销文件、组织机构代码注销文件',
        isneed:true,
        single:false
    },
    {
        type:'48',
        name:'其他相关材料附件(非必填)',
        isneed:false,
        single:false
    }
]//申请注销网点


//变更登记旅行社所需材料附件
var agencyChangeApplyFileList = [
    {
        type:'1',
        name:'营业执照原件',
        isneed:true,
        single:false
    },
    {
        type:'50',
        name:'营业执照复印件',
        isneed:true,
        single:false
    },
    {
        type:'5',
        name:'企业章程原件',
        isneed:true,
        single:false
    },
    {
        type:'6',
        name:'法定代表人身份证正面',
        isneed:true,
        single:false
    },
    {
        type:'7',
        name:'法定代表人身份证反面',
        isneed:true,
        single:false
    },
    {
        type:'8',
        name:'旅行社经营业务许可证原件正本',
        isneed:true,
        single:false
    },
    {
        type:'9',
        name:'旅行社经营业务许可证原件副本',
        isneed:true,
        single:false
    },
    {
        type:'51',
        name:'变更登记其他相关材料附件',
        isneed:false,
        single:false
    }
];


