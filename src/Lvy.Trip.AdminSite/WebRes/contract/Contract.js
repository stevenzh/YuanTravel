define(['jquery'],function ($) {
    var Contract = {
        createNew: function () {
            return {
                dayList: [],
                selfFormalities:$("#selfFormalities")?verdict($("#selfFormalities")):"",
                travelAgency:{
                    localComplaintPhone: $('#localComplaintPhone') ? verdict($('#localComplaintPhone')) : "",
                    localConsumerAssociationPhone: $('#localConsumerAssociationPhone') ? verdict($('#localConsumerAssociationPhone')) : "",
                    outletName:$("#outletName")?verdict($("#outletName")):"",
                    IM:$("#travelAgencyIM")?verdict($("#travelAgencyIM")):"",
                    email:$("#travelAgencyEmail")?verdict($("#travelAgencyEmail")):"",
                    fax:$("#travelAgencyFax")?verdict($("#travelAgencyFax")):"",
                },
                signatory:{
                signingPlace:$("#signingPlace")?verdict($("#signingPlace")):"",
              },
                //游客代表信息
                itinerary: {
                    inputType: 0,
                    days:$("#itineraryDays") ? verdict($("#itineraryDays")):"",
                    nights:$("#itineraryNights") ? verdict($("#itineraryNights")):"",
                    groupMode: $("input[name='groupAgreementMode']") ? 1 : "",
                    tourGuideServiceType:$("#tourGuideServiceType") ? verdict($("#tourGuideServiceType")):1,
                    routeName: $("#routeName") ? verdict($("#routeName")):"",
                    groupId: $("#groupId")?verdict($("#groupId")):"",
                    startDate: $("#startDate")?verdict($("#startDate")):"",
                    endDate: $("#endDate")?verdict($("#endDate")):"",
                    gatherMethod: $("input[name='gatherMethod']") ? 1 :"",
                    gatherTime: $("#gatherTime")? verdict($("#gatherTime")) :"",
                    gatherTimeLimit: $("#gatherTimeLimit")? verdict($("#gatherTimeLimit")) :"",
                    gatherPlace: $("#gatherPlace")? verdict($("#gatherPlace")) :"",
                    dismissPlace: $("#dismissPlace")? verdict($("#dismissPlace")) :"",
                    departureCity: {
                        description: $("#departureCityDescription")?verdict($("#departureCityDescription")):"",
                    },
                    passCity: {
                        description: $("#passCityDescription")?verdict($("#passCityDescription")):"",
                    },
                    arrivalCity: {
                        description: $("#arrivalCityDescription")?verdict($("#arrivalCityDescription")):"",
                    },
                    endCity: {
                        description:$("#endCityDescription")?verdict($("#endCityDescription")):"",
                    },
                    attraction: {
                        description: $("#attractionDescription")?verdict($("#attractionDescription")):"",
                    },
                    longTransport: {
                        description: $("#longTransportDescription")?verdict($("#longTransportDescription")):"",
                        standard: $("#longTransportStandard")?verdict($("#longTransportStandard")):"",
                    },
                    localTransport: {
                        description: $("#localTransportDescription")?verdict($("#localTransportDescription")):"",
                        standard: $("#localTransportStandard")?verdict($("#localTransportStandard")):"",
                    },
                    freeTime: {
                        description: $("#freeTimeDescription")?verdict($("#freeTimeDescription")):"",
                        number: $("#freeTimeNumber")?verdict($("#freeTimeNumber")):"",
                    },
                    hotel: {
                        description: $("#hotelDescription")?verdict($("#hotelDescription")):"",
                    },
                    meal: {
                        number: $("#mealNumber")?verdict($("#mealNumber")):"",
                        standard: $("#mealStandard")?verdict($("#mealStandard")):"",
                        isAtOwnExpense: $("#isAtOwnExpense")? true :"",
                    },
                    lunch:{
                        has: $("#hasLunch")? false :"",
                        standard: $("#lunchStandard")?verdict($("#lunchStandard")):""
                    },
                    dinner:{
                        has: $("#hasDinner")? false :"",
                        standard: $("#dinnerStandard")?verdict($("#dinnerStandard")):""
                    },
                    onboard:{
                        description:$("#onboardDescription")?verdict($("#onboardDescription")):"",
                        meal:{
                            number:$("#onboardMealNumber")?verdict($("#onboardMealNumber")):"",
                            standard:$("#onboardMealStandard")?verdict($("#onboardMealStandard")):""
                        }
                    }
                },
                dispute:{
                    tribunalName:$('#tribunalName') ? verdict($('#tribunalName')) : "",
                    resolution:$('#resolution') ? verdict($('#resolution')) : ""
                },
                localTravelAgencies: [
                    {
                        agencyName: $("#localTravelAgenciesAgencyName")?verdict($("#localTravelAgenciesAgencyName")):"",
                        agencyAddress: {
                            description: $("#localTravelAgenciesAgencyAddress")?verdict($("#localTravelAgenciesAgencyAddress")):"",
                        },
                        contactName: $("#localTravelAgenciesContactName")?verdict($("#localTravelAgenciesContactName")):"",
                        contactPhone: $("#localTravelAgenciesContactPhone")?verdict($("#localTravelAgenciesContactPhone")):"",
                    },
                    {
                      agencyName:$("#localTravelAgenciesName1")?verdict($("#localTravelAgenciesName1")):"",
                    },
                    {
                      agencyName:$("#localTravelAgenciesName2")?verdict($("#localTravelAgenciesName2")):"",
                    }
                ],
                cost: {
                    totalCost:$("#totalCost")?verdict($("#totalCost")):"",
                    guideServiceCost: $("#guideServiceCost")?verdict($("#guideServiceCost")):"",
                    totalGuideServiceCost: $("#totalGuideServiceCost")?verdict($("#totalGuideServiceCost")):"",
                    deadline: $("#deadline")?verdict($("#deadline")):"",
                    paymentMethodDescription: $("#paymentOtherDescription")?verdict($("#paymentOtherDescription")):"",
                    includeNote:[],
                    excludeNote:$("#excludeNote")?verdict($("#excludeNote")):"",
                    otherCost:$("#otherCost")?verdict($("#otherCost")):"",
                    paymentMethod: $("#paymentMethod")?verdict($("#paymentMethod")): 1,
                    paymentTime: $("#paymentTime")?verdict($("#paymentTime")): 1,
                    overduePenaltyDayRatio: $("#overduePenaltyDayRatio")?verdict($("#overduePenaltyDayRatio")):"",
                    overduePenalty: $("#overduePenalty")?verdict($("#overduePenalty")):"",
                    adultCost: $("#adultCost")? verdict($("#adultCost")) : "",
                    childCost: $("#childCost")? verdict($("#childCost")) : "",
                    needDeposit: $("#needDeposit")? verdict($("#needDeposit")) : "",
                    deposit: $("#deposit")? verdict($("#deposit")) : "",
                    depositPaySolution: $("#depositPaySolution")? verdict($("#depositPaySolution")) : "",
                    singleSupplementSolutionDescription: $("#singleSupplementSolutionDescription")? verdict($("#singleSupplementSolutionDescription")) : ""
                },
                insurance: {
                    purchaseMethod:$("#purchaseMethod")?verdict($("#purchaseMethod")): 1,
                    agreeToBuy:$("#agreeToBuy")?verdict($("#agreeToBuy")): 1,
                    company: $("#insuranceCompany")?verdict($("#insuranceCompany")):"",
                    productName: $("#insuranceProduct")?verdict($("#insuranceProduct")):"",
                    payer: $("#insurancePayer")?verdict($("#insurancePayer")):"",
                    deadline: $("#insuranceDeadline")?verdict($("#insuranceDeadline")):"",
                    coverage: $("#coverage")?verdict($("#coverage")):"",
                    premium: $("#premium")?verdict($("#premium")):"",
                    agreeToBuyInsurance:{
                        cruiseTravelAccident:$("#cruiseTravelAccident") ? verdict($("#cruiseTravelAccident")):1,
                        personalAccident:$("#personalAccident") ? verdict($("#personalAccident")):1
                    }
                },
                groupAgreement: {
                    mergeToCompanyName: $("#groupAgreementMergeToCompanyName")?verdict($("#groupAgreementMergeToCompanyName")):"",
                    resolution: $("#groupAgreementResolution") ? verdict($("#groupAgreementResolution")) : 1,
                    leastCustomerNumber:$("#leastCustomerNumber")?verdict($("#leastCustomerNumber")):"",
                    compensationPeriod: $("#compensationPeriod")?verdict($("#compensationPeriod")):"",
                    seriousCompensateRatio: $("#seriousCompensateRatio")?verdict($("#seriousCompensateRatio")):"",
                    otherLiability: $("#otherLiability")?verdict($("#otherLiability")):"",
                    changeRouteResolution:$("#changeRouteResolution")?verdict($("#changeRouteResolution")):1,
                    remainMoney:$("#remainMoney")?verdict($("#remainMoney")):"",
                    compensationRatioList:[
                        {
                            ratio:$("#compensationRatioListRatio")?verdict($("#compensationRatioListRatio")):"",
                        }
                    ],
                    shopping:{
                        penaltyStandard:{
                            ratio:$("#shoppingPenaltyStandardRatio")?verdict($("#shoppingPenaltyStandardRatio")):"",
                        }
                    },
                  agreeTransfer: $('#agreeTransfer') ? verdict($('#agreeTransfer')) : 1,
                  agreeToDelay: $('#agreeToDelay') ? verdict($('#agreeToDelay')) : 1,
                  agreeToChangeRoute: $('#agreeToChangeRoute') ? verdict($('#agreeToChangeRoute')) : 1,
                  agreeToCancel: $('#agreeToCancel') ? verdict($('#agreeToCancel')) : 1,
                  transferToCompanyName: $('#transferToCompanyName') ? verdict($('#transferToCompanyName')) : "",
                  agreeToMerge: $('#groupAgree') ? verdict($('#groupAgree')) : "",
                    partAPenalty: {
                        solution: $("#partAPenaltySolution") ? verdict($("#partAPenaltySolution")) : 1,
                        overdueDaysB: $("#overdueDaysB") ? verdict($("#overdueDaysB")) : "",
                        overdueDaysA: $("#overdueDaysA") ? verdict($("#overdueDaysA")) : "",
                        cancel: {
                            standards: [
                                {
                                    start: $("#partAStart0") ? verdict($("#partAStart0")) : "",
                                    end: $("#partAEnd0") ? verdict($("#partAEnd0")) : "",
                                    ratio: $("#partARatio0") ? verdict($("#partARatio0")) : "",
                                }, {
                                    start: $("#partAStart1") ? verdict($("#partAStart1")) : "",
                                    end: $("#partAEnd1") ? verdict($("#partAEnd1")) : "",
                                    ratio: $("#partARatio1") ? verdict($("#partARatio1")) : "",
                                }, {
                                    start: $("#partAStart2") ? verdict($("#partAStart2")) : "",
                                    end: $("#partAEnd2") ? verdict($("#partAEnd2")) : "",
                                    ratio: $("#partARatio2") ? verdict($("#partARatio2")) : "",
                                }, {
                                    start: $("#partAStart3") ? verdict($("#partAStart3")) : "",
                                    end: $("#partAEnd3") ? verdict($("#partAEnd3")) : "",
                                    ratio: $("#partARatio3") ? verdict($("#partARatio3")) : "",
                                }, {
                                    start: $("#partAStart4") ? verdict($("#partAStart4")) : "",
                                    end: $("#partAEnd4") ? verdict($("#partAEnd4")) : "",
                                    ratio: $("#partARatio4") ? verdict($("#partARatio4")) : "",
                                },
                            ],
                            timeLimit: $("#timeLimit") ? verdict($("#timeLimit")) : "",

                        },
                        interrupt: {
                            formula: [
                                $("#partAformula0")?verdict($("#partAformula0")):"",
                                $("#partAformula1")?verdict($("#partAformula1")):"",
                                $("#partAformula2")?verdict($("#partAformula2")):"",
                                $("#partAformula3")?verdict($("#partAformula3")):"",
                            ],
                            timeLimit: $("#interruptTimeLimit") ? verdict($("#interruptTimeLimit")) : "",
                        }
                    },
                    partBPenalty: {
                        solution: $("#partBPenaltySolution") ? verdict($("#partBPenaltySolution")) : 1,
                        cancel: {
                            standards: [
                                {
                                    start: $("#partBStart0") ? verdict($("#partBStart0")) : "",
                                    end: $("#partBEnd0") ? verdict($("#partBEnd0")) : "",
                                    ratio: $("#partBRatio0") ? verdict($("#partBRatio0")) : "",
                                }, {
                                    start: $("#partBStart1") ? verdict($("#partBStart1")) : "",
                                    end: $("#partBEnd1") ? verdict($("#partBEnd1")) : "",
                                    ratio: $("#partBRatio1") ? verdict($("#partBRatio1")) : "",
                                }, {
                                    start: $("#partBStart2") ? verdict($("#partBStart2")) : "",
                                    end: $("#partBEnd2") ? verdict($("#partBEnd2")) : "",
                                    ratio: $("#partBRatio2") ? verdict($("#partBRatio2")) : "",
                                }, {
                                    start: $("#partBStart3") ? verdict($("#partBStart3")) : "",
                                    end: $("#partBEnd3") ? verdict($("#partBEnd3")) : "",
                                    ratio: $("#partBRatio3") ? verdict($("#partBRatio3")) : "",
                                }, {
                                    start: $("#partBStart4") ? verdict($("#partBStart4")) : "",
                                    end: $("#partBEnd4") ? verdict($("#partBEnd4")) : "",
                                    ratio: $("#partBRatio4") ? verdict($("#partBRatio4")) : "",
                                },
                            ]
                        }
                    }
                },
                touristsInfo:{
                    childNumber: $("#childNumber")? verdict($("#childNumber")) : "",
                    adultNumber: $("#adultNum")? verdict($("#adultNum")) : "",
                    healthOption: $("#healthOption") ? verdict($("#healthOption")) : "",
                    healthDescription: $("#healthDescription") ? verdict($("#healthDescription")) : "",
                    room:{
                        partnerDescription:$("#partnerDescription") ? verdict($("#partnerDescription")) : "",
                        additionalDescription:$("#additionalDescription") ? verdict($("#additionalDescription")) : "",
                        noBedDescription:$("#noBedDescription") ? verdict($("#noBedDescription")) : "",
                        singleSupplementDescription:$("#singleSupplementDescription") ? verdict($("#singleSupplementDescription")) : "",
                    }
                },
                supplementaryClause: $("#additionContent") ? verdict($("#additionContent")):"",
                enrollmentSupplementaryClause: $("#enrollmentSupplementaryClause") ? verdict($("#enrollmentSupplementaryClause")):"",
                entrustedTravelAgency: {
                    agencyName: $("#entrustedTravelAgencyAgencyName")?verdict($("#entrustedTravelAgencyAgencyName")):"",
                    travelAgencyLicenseNumber:$("#travelAgencyLicenseNumber")?verdict($("#travelAgencyLicenseNumber")):"",
                },
                entrustment: {
                    order:{
                        tourGuideService:{
                            needNationalGuide: $("#needNationalGuide")?verdict($("#needNationalGuide")):""
                        },
                        agreeToDelegate: $("#agreeToDelegate")?verdict($("#agreeToDelegate")):""
                    }
                },
            }
        }
    };
    return Contract;
})