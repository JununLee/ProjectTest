///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              DCM2Volume, DICOM
/// Description:        Loading DICOM files into a volume object.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
///-----------------------------------------------------------------

#if !NETFX_CORE
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using UnityEngine;

namespace VolumeViewer
{
    public struct DICOM
    {
        public const int LSB_FIRST = 1;
        public const int MSB_FIRST = 2;

        public enum FormatCode
        {
            DICOM_TYPE_UNKNOWN = 0,
            DICOM_TYPE_BINARY = 1,
            DICOM_TYPE_UINT8 = 2,
            DICOM_TYPE_INT16 = 4,
            DICOM_TYPE_INT32 = 8,
            DICOM_TYPE_FLOAT32 = 16,
            DICOM_TYPE_RGB24 = 128,
            DICOM_TYPE_INT8 = 256,
            DICOM_TYPE_UINT16 = 512,
            DICOM_TYPE_UINT32 = 768,
            DICOM_TYPE_RGBA32 = 2304
        }

        public enum PhotometricInterpretationCode
        {
            MONOCHROME1,
            MONOCHROME2,
            PALETTE_COLOR,
            RGB,
            HSV,
            YBR_FULL,
            ARGB,
            CMYK,
            YBR_FULL_422,
            YBR_PARTIAL_422,
            YBR_PARTIAL_420,
            YBR_ICT,
            YBR_RCT
        }

        public static Dictionary<UInt32, string> dicomDict = new Dictionary<UInt32, string>{
                                                            { 0, "ULGroupLength" },
                                                            { 65536, "ULCommandLengthToEnd" },
                                                            { 131072, "UIAffectedSOPClassUID" },
                                                            { 196608, "UIRequestedSOPClassUID" },
                                                            { 1048576, "LTCommandRecognitionCode" },
                                                            { 16777216, "USCommandField" },
                                                            { 17825792, "USMessageID" },
                                                            { 18874368, "USMessageIDBeingRespondedTo" },
                                                            { 33554432, "AEInitiator" },
                                                            { 50331648, "AEReceiver" },
                                                            { 67108864, "AEFindLocation" },
                                                            { 100663296, "AEMoveDestination" },
                                                            { 117440512, "USPriority" },
                                                            { 134217728, "USDataSetType" },
                                                            { 139460608, "USNumberOfMatches" },
                                                            { 140509184, "USResponseSequenceNumber" },
                                                            { 150994944, "USStatus" },
                                                            { 151060480, "ATOffendingElement" },
                                                            { 151126016, "LOErrorComment" },
                                                            { 151191552, "USErrorID" },
                                                            { 268435456, "UIAffectedSOPInstanceUID" },
                                                            { 268500992, "UIRequestedSOPInstanceUID" },
                                                            { 268566528, "USEventTypeID" },
                                                            { 268763136, "ATAttributeIdentifierList" },
                                                            { 268959744, "USActionTypeID" },
                                                            { 270532608, "USNumberOfRemainingSuboperations" },
                                                            { 270598144, "USNumberOfCompletedSuboperations" },
                                                            { 270663680, "USNumberOfFailedSuboperations" },
                                                            { 270729216, "USNumberOfWarningSuboperations" },
                                                            { 271581184, "AEMoveOriginatorApplicationEntityTitle" },
                                                            { 271646720, "USMoveOriginatorMessageID" },
                                                            { 1073741824, "LTDialogReceiver" },
                                                            { 1074790400, "LTTerminalType" },
                                                            { 1343225856, "SHMessageSetID" },
                                                            { 1344274432, "SHEndMessageSet" },
                                                            { 1360003072, "LTDisplayFormat" },
                                                            { 1361051648, "LTPagePositionID" },
                                                            { 1362100224, "LTTextFormatID" },
                                                            { 1363148800, "LTNormalReverse" },
                                                            { 1364197376, "LTAddGrayScale" },
                                                            { 1365245952, "LTBorders" },
                                                            { 1366294528, "ISCopies" },
                                                            { 1367343104, "LTOldMagnificationType" },
                                                            { 1368391680, "LTErase" },
                                                            { 1369440256, "LTPrint" },
                                                            { 1370488832, "USOverlays" },
                                                            { 2, "ULFileMetaInformationGroupLength" },
                                                            { 65538, "OBFileMetaInformationVersion" },
                                                            { 131074, "UIMediaStorageSOPClassUID" },
                                                            { 196610, "UIMediaStorageSOPInstanceUID" },
                                                            { 1048578, "UITransferSyntaxUID" },
                                                            { 1179650, "UIImplementationClassUID" },
                                                            { 1245186, "SHImplementationVersionName" },
                                                            { 1441794, "AESourceApplicationEntityTitle" },
                                                            { 1507330, "AESendingApplicationEntityTitle" },
                                                            { 1572866, "AEReceivingApplicationEntityTitle" },
                                                            { 16777218, "UIPrivateInformationCreatorUID" },
                                                            { 16908290, "OBPrivateInformation" },
                                                            { 288358404, "CSFileSetID" },
                                                            { 289472516, "CSFileSetDescriptorFileID" },
                                                            { 289538052, "CSSpecificCharacterSetOfFileSetDescriptorFile" },
                                                            { 301989892, "ULOffsetOfTheFirstDirectoryRecordOfTheRootDirectoryEntity" },
                                                            { 302120964, "ULOffsetOfTheLastDirectoryRecordOfTheRootDirectoryEntity" },
                                                            { 303169540, "USFileSetConsistencyFlag" },
                                                            { 304087044, "SQDirectoryRecordSequence" },
                                                            { 335544324, "ULOffsetOfTheNextDirectoryRecord" },
                                                            { 336592900, "USRecordInUseFlag" },
                                                            { 337641476, "ULOffsetOfReferencedLowerLevelDirectoryEntity" },
                                                            { 338690052, "CSDirectoryRecordType" },
                                                            { 338821124, "UIPrivateRecordUID" },
                                                            { 352321540, "CSReferencedFileID" },
                                                            { 352583684, "ULMRDRDirectoryRecordOffset" },
                                                            { 353370116, "UIReferencedSOPClassUIDInFile" },
                                                            { 353435652, "UIReferencedSOPInstanceUIDInFile" },
                                                            { 353501188, "UIReferencedTransferSyntaxUIDInFile" },
                                                            { 354025476, "UIReferencedRelatedGeneralSOPClassUIDInFile" },
                                                            { 369098756, "ULNumberOfReferences" },
                                                            { 65544, "ULLengthToEnd" },
                                                            { 327688, "CSSpecificCharacterSet" },
                                                            { 393224, "SQLanguageCodeSequence" },
                                                            { 524296, "CSImageType" },
                                                            { 1048584, "SHRecognitionCode" },
                                                            { 1179656, "DAInstanceCreationDate" },
                                                            { 1245192, "TMInstanceCreationTime" },
                                                            { 1310728, "UIInstanceCreatorUID" },
                                                            { 1376264, "DTInstanceCoercionDateTime" },
                                                            { 1441800, "UISOPClassUID" },
                                                            { 1572872, "UISOPInstanceUID" },
                                                            { 1703944, "UIRelatedGeneralSOPClassUID" },
                                                            { 1769480, "UIOriginalSpecializedSOPClassUID" },
                                                            { 2097160, "DAStudyDate" },
                                                            { 2162696, "DASeriesDate" },
                                                            { 2228232, "DAAcquisitionDate" },
                                                            { 2293768, "DAContentDate" },
                                                            { 2359304, "DAOverlayDate" },
                                                            { 2424840, "DACurveDate" },
                                                            { 2752520, "DTAcquisitionDateTime" },
                                                            { 3145736, "TMStudyTime" },
                                                            { 3211272, "TMSeriesTime" },
                                                            { 3276808, "TMAcquisitionTime" },
                                                            { 3342344, "TMContentTime" },
                                                            { 3407880, "TMOverlayTime" },
                                                            { 3473416, "TMCurveTime" },
                                                            { 4194312, "USDataSetType" },
                                                            { 4259848, "LODataSetSubtype" },
                                                            { 4325384, "CSNuclearMedicineSeriesType" },
                                                            { 5242888, "SHAccessionNumber" },
                                                            { 5308424, "SQIssuerOfAccessionNumberSequence" },
                                                            { 5373960, "CSQueryRetrieveLevel" },
                                                            { 5439496, "CSQueryRetrieveView" },
                                                            { 5505032, "AERetrieveAETitle" },
                                                            { 5570568, "AEStationAETitle" },
                                                            { 5636104, "CSInstanceAvailability" },
                                                            { 5767176, "UIFailedSOPInstanceUIDList" },
                                                            { 6291464, "CSModality" },
                                                            { 6357000, "CSModalitiesInStudy" },
                                                            { 6422536, "UISOPClassesInStudy" },
                                                            { 6553608, "CSConversionType" },
                                                            { 6815752, "CSPresentationIntentType" },
                                                            { 7340040, "LOManufacturer" },
                                                            { 8388616, "LOInstitutionName" },
                                                            { 8454152, "STInstitutionAddress" },
                                                            { 8519688, "SQInstitutionCodeSequence" },
                                                            { 9437192, "PNReferringPhysicianName" },
                                                            { 9568264, "STReferringPhysicianAddress" },
                                                            { 9699336, "SHReferringPhysicianTelephoneNumbers" },
                                                            { 9830408, "SQReferringPhysicianIdentificationSequence" },
                                                            { 10223624, "PNConsultingPhysicianName" },
                                                            { 10289160, "SQConsultingPhysicianIdentificationSequence" },
                                                            { 16777224, "SHCodeValue" },
                                                            { 16842760, "LOExtendedCodeValue" },
                                                            { 16908296, "SHCodingSchemeDesignator" },
                                                            { 16973832, "SHCodingSchemeVersion" },
                                                            { 17039368, "LOCodeMeaning" },
                                                            { 17104904, "CSMappingResource" },
                                                            { 17170440, "DTContextGroupVersion" },
                                                            { 17235976, "DTContextGroupLocalVersion" },
                                                            { 17301512, "LTExtendedCodeMeaning" },
                                                            { 17498120, "CSContextGroupExtensionFlag" },
                                                            { 17563656, "UICodingSchemeUID" },
                                                            { 17629192, "UIContextGroupExtensionCreatorUID" },
                                                            { 17760264, "CSContextIdentifier" },
                                                            { 17825800, "SQCodingSchemeIdentificationSequence" },
                                                            { 17956872, "LOCodingSchemeRegistry" },
                                                            { 18087944, "STCodingSchemeExternalID" },
                                                            { 18153480, "STCodingSchemeName" },
                                                            { 18219016, "STCodingSchemeResponsibleOrganization" },
                                                            { 18284552, "UIContextUID" },
                                                            { 18350088, "UIMappingResourceUID" },
                                                            { 18415624, "UCLongCodeValue" },
                                                            { 18874376, "URURNCodeValue" },
                                                            { 18939912, "SQEquivalentCodeSequence" },
                                                            { 19005448, "LOMappingResourceName" },
                                                            { 19070984, "SQContextGroupIdentificationSequence" },
                                                            { 19136520, "SQMappingResourceIdentificationSequence" },
                                                            { 33619976, "SHTimezoneOffsetFromUTC" },
                                                            { 35651592, "SQResponsibleGroupCodeSequence" },
                                                            { 35717128, "CSEquipmentModality" },
                                                            { 35782664, "LOManufacturerRelatedModelGroup" },
                                                            { 50331656, "SQPrivateDataElementCharacteristicsSequence" },
                                                            { 50397192, "USPrivateGroupReference" },
                                                            { 50462728, "LOPrivateCreatorReference" },
                                                            { 50528264, "CSBlockIdentifyingInformationStatus" },
                                                            { 50593800, "USNonidentifyingPrivateElements" },
                                                            { 50724872, "USIdentifyingPrivateElements" },
                                                            { 50659336, "SQDeidentificationActionSequence" },
                                                            { 50790408, "CSDeidentificationAction" },
                                                            { 50855944, "USPrivateDataElement" },
                                                            { 50921480, "ULPrivateDataElementValueMultiplicity" },
                                                            { 50987016, "CSPrivateDataElementValueRepresentation" },
                                                            { 51052552, "ULPrivateDataElementNumberOfItems" },
                                                            { 51118088, "UCPrivateDataElementName" },
                                                            { 51183624, "UCPrivateDataElementKeyword" },
                                                            { 51249160, "UTPrivateDataElementDescription" },
                                                            { 51314696, "UTPrivateDataElementEncoding" },
                                                            { 51380232, "SQPrivateDataElementDefinitionSequence" },
                                                            { 268435464, "AENetworkID" },
                                                            { 269484040, "SHStationName" },
                                                            { 271581192, "LOStudyDescription" },
                                                            { 271712264, "SQProcedureCodeSequence" },
                                                            { 272498696, "LOSeriesDescription" },
                                                            { 272564232, "SQSeriesDescriptionCodeSequence" },
                                                            { 272629768, "LOInstitutionalDepartmentName" },
                                                            { 273154056, "PNPhysiciansOfRecord" },
                                                            { 273219592, "SQPhysiciansOfRecordIdentificationSequence" },
                                                            { 273678344, "PNPerformingPhysicianName" },
                                                            { 273809416, "SQPerformingPhysicianIdentificationSequence" },
                                                            { 274726920, "PNNameOfPhysiciansReadingStudy" },
                                                            { 274857992, "SQPhysiciansReadingStudyIdentificationSequence" },
                                                            { 275775496, "PNOperatorsName" },
                                                            { 275906568, "SQOperatorIdentificationSequence" },
                                                            { 276824072, "LOAdmittingDiagnosesDescription" },
                                                            { 277086216, "SQAdmittingDiagnosesCodeSequence" },
                                                            { 277872648, "LOManufacturerModelName" },
                                                            { 285212680, "SQReferencedResultsSequence" },
                                                            { 286261256, "SQReferencedStudySequence" },
                                                            { 286326792, "SQReferencedPerformedProcedureStepSequence" },
                                                            { 286588936, "SQReferencedSeriesSequence" },
                                                            { 287309832, "SQReferencedPatientSequence" },
                                                            { 287637512, "SQReferencedVisitSequence" },
                                                            { 288358408, "SQReferencedOverlaySequence" },
                                                            { 288620552, "SQReferencedStereometricInstanceSequence" },
                                                            { 289013768, "SQReferencedWaveformSequence" },
                                                            { 289406984, "SQReferencedImageSequence" },
                                                            { 289734664, "SQReferencedCurveSequence" },
                                                            { 290062344, "SQReferencedInstanceSequence" },
                                                            { 290127880, "SQReferencedRealWorldValueMappingInstanceSequence" },
                                                            { 290455560, "UIReferencedSOPClassUID" },
                                                            { 290783240, "UIReferencedSOPInstanceUID" },
                                                            { 291110920, "UISOPClassesSupported" },
                                                            { 291504136, "ISReferencedFrameNumber" },
                                                            { 291569672, "ULSimpleFrameList" },
                                                            { 291635208, "ULCalculatedFrameList" },
                                                            { 291700744, "FDTimeRange" },
                                                            { 291766280, "SQFrameExtractionSequence" },
                                                            { 291962888, "UIMultiFrameSourceSOPInstanceUID" },
                                                            { 294649864, "URRetrieveURL" },
                                                            { 294977544, "UITransactionUID" },
                                                            { 295043080, "USWarningReason" },
                                                            { 295108616, "USFailureReason" },
                                                            { 295174152, "SQFailedSOPSequence" },
                                                            { 295239688, "SQReferencedSOPSequence" },
                                                            { 295305224, "SQOtherFailuresSequence" },
                                                            { 301989896, "SQStudiesContainingOtherReferencedInstancesSequence" },
                                                            { 307232776, "SQRelatedSeriesSequence" },
                                                            { 554696712, "CSLossyImageCompressionRetired" },
                                                            { 554762248, "STDerivationDescription" },
                                                            { 554827784, "SQSourceImageSequence" },
                                                            { 555745288, "SHStageName" },
                                                            { 555876360, "ISStageNumber" },
                                                            { 556007432, "ISNumberOfStages" },
                                                            { 556204040, "SHViewName" },
                                                            { 556269576, "ISViewNumber" },
                                                            { 556335112, "ISNumberOfEventTimers" },
                                                            { 556400648, "ISNumberOfViewsInStage" },
                                                            { 556793864, "DSEventElapsedTimes" },
                                                            { 556924936, "LOEventTimerNames" },
                                                            { 556990472, "SQEventTimerSequence" },
                                                            { 557056008, "FDEventTimeOffset" },
                                                            { 557121544, "SQEventCodeSequence" },
                                                            { 557973512, "ISStartTrim" },
                                                            { 558039048, "ISStopTrim" },
                                                            { 558104584, "ISRecommendedDisplayFrameRate" },
                                                            { 570425352, "CSTransducerPosition" },
                                                            { 570687496, "CSTransducerOrientation" },
                                                            { 570949640, "CSAnatomicStructure" },
                                                            { 571998216, "SQAnatomicRegionSequence" },
                                                            { 572522504, "SQAnatomicRegionModifierSequence" },
                                                            { 573046792, "SQPrimaryAnatomicStructureSequence" },
                                                            { 573112328, "SQAnatomicStructureSpaceOrRegionSequence" },
                                                            { 573571080, "SQPrimaryAnatomicStructureModifierSequence" },
                                                            { 574619656, "SQTransducerPositionSequence" },
                                                            { 574750728, "SQTransducerPositionModifierSequence" },
                                                            { 574881800, "SQTransducerOrientationSequence" },
                                                            { 575012872, "SQTransducerOrientationModifierSequence" },
                                                            { 575733768, "SQAnatomicStructureSpaceOrRegionCodeSequenceTrial" },
                                                            { 575864840, "SQAnatomicPortalOfEntranceCodeSequenceTrial" },
                                                            { 575995912, "SQAnatomicApproachDirectionCodeSequenceTrial" },
                                                            { 576061448, "STAnatomicPerspectiveDescriptionTrial" },
                                                            { 576126984, "SQAnatomicPerspectiveCodeSequenceTrial" },
                                                            { 576192520, "STAnatomicLocationOfExaminingInstrumentDescriptionTrial" },
                                                            { 576258056, "SQAnatomicLocationOfExaminingInstrumentCodeSequenceTrial" },
                                                            { 576323592, "SQAnatomicStructureSpaceOrRegionModifierCodeSequenceTrial" },
                                                            { 576454664, "SQOnAxisBackgroundAnatomicStructureCodeSequenceTrial" },
                                                            { 805371912, "SQAlternateRepresentationSequence" },
                                                            { 806354952, "UIIrradiationEventUID" },
                                                            { 806420488, "SQSourceIrradiationEventSequence" },
                                                            { 806486024, "UIRadiopharmaceuticalAdministrationEventUID" },
                                                            { 1073741832, "LTIdentifyingComments" },
                                                            { 2416377864, "CSFrameType" },
                                                            { 2425487368, "SQReferencedImageEvidenceSequence" },
                                                            { 2434859016, "SQReferencedRawDataSequence" },
                                                            { 2434990088, "UICreatorVersionUID" },
                                                            { 2435055624, "SQDerivationImageSequence" },
                                                            { 2438201352, "SQSourceImageEvidenceSequence" },
                                                            { 2449801224, "CSPixelPresentation" },
                                                            { 2449866760, "CSVolumetricProperties" },
                                                            { 2449932296, "CSVolumeBasedCalculationTechnique" },
                                                            { 2449997832, "CSComplexImageComponent" },
                                                            { 2450063368, "CSAcquisitionContrast" },
                                                            { 2450849800, "SQDerivationCodeSequence" },
                                                            { 2453078024, "SQReferencedPresentationStateSequence" },
                                                            { 2484076552, "SQReferencedOtherPlaneSequence" },
                                                            { 2488795144, "SQFrameDisplaySequence" },
                                                            { 2488860680, "FLRecommendedDisplayFrameRateInFloat" },
                                                            { 2489319432, "CSSkipFrameRangeFlag" },
                                                            { 1048592, "PNPatientName" },
                                                            { 2097168, "LOPatientID" },
                                                            { 2162704, "LOIssuerOfPatientID" },
                                                            { 2228240, "CSTypeOfPatientID" },
                                                            { 2359312, "SQIssuerOfPatientIDQualifiersSequence" },
                                                            { 2490384, "SQSourcePatientGroupIdentificationSequence" },
                                                            { 2555920, "SQGroupOfPatientsIdentificationSequence" },
                                                            { 2621456, "USSubjectRelativePositionInImage" },
                                                            { 3145744, "DAPatientBirthDate" },
                                                            { 3276816, "TMPatientBirthTime" },
                                                            { 3342352, "LOPatientBirthDateInAlternativeCalendar" },
                                                            { 3407888, "LOPatientDeathDateInAlternativeCalendar" },
                                                            { 3473424, "CSPatientAlternativeCalendar" },
                                                            { 4194320, "CSPatientSex" },
                                                            { 5242896, "SQPatientInsurancePlanCodeSequence" },
                                                            { 16842768, "SQPatientPrimaryLanguageCodeSequence" },
                                                            { 16908304, "SQPatientPrimaryLanguageModifierCodeSequence" },
                                                            { 33554448, "CSQualityControlSubject" },
                                                            { 33619984, "SQQualityControlSubjectTypeCodeSequence" },
                                                            { 34734096, "UCStrainDescription" },
                                                            { 34799632, "LOStrainNomenclature" },
                                                            { 34865168, "LOStrainStockNumber" },
                                                            { 34930704, "SQStrainSourceRegistryCodeSequence" },
                                                            { 34996240, "SQStrainStockSequence" },
                                                            { 35061776, "LOStrainSource" },
                                                            { 35127312, "UTStrainAdditionalInformation" },
                                                            { 35192848, "SQStrainCodeSequence" },
                                                            { 268435472, "LOOtherPatientIDs" },
                                                            { 268501008, "PNOtherPatientNames" },
                                                            { 268566544, "SQOtherPatientIDsSequence" },
                                                            { 268763152, "PNPatientBirthName" },
                                                            { 269484048, "ASPatientAge" },
                                                            { 270532624, "DSPatientSize" },
                                                            { 270598160, "SQPatientSizeCodeSequence" },
                                                            { 270663696, "DSPatientBodyMassIndex" },
                                                            { 270729232, "DSMeasuredAPDimension" },
                                                            { 270794768, "DSMeasuredLateralDimension" },
                                                            { 271581200, "DSPatientWeight" },
                                                            { 272629776, "LOPatientAddress" },
                                                            { 273678352, "LOInsurancePlanIdentification" },
                                                            { 274726928, "PNPatientMotherBirthName" },
                                                            { 276824080, "LOMilitaryRank" },
                                                            { 276889616, "LOBranchOfService" },
                                                            { 277872656, "LOMedicalRecordLocator" },
                                                            { 285212688, "SQReferencedPatientPhotoSequence" },
                                                            { 536870928, "LOMedicalAlerts" },
                                                            { 554696720, "LOAllergies" },
                                                            { 558891024, "LOCountryOfResidence" },
                                                            { 559022096, "LORegionOfResidence" },
                                                            { 559153168, "SHPatientTelephoneNumbers" },
                                                            { 559218704, "LTPatientTelecomInformation" },
                                                            { 559939600, "SHEthnicGroup" },
                                                            { 562036752, "SHOccupation" },
                                                            { 564133904, "CSSmokingStatus" },
                                                            { 565182480, "LTAdditionalPatientHistory" },
                                                            { 566231056, "USPregnancyStatus" },
                                                            { 567279632, "DALastMenstrualDate" },
                                                            { 569376784, "LOPatientReligiousPreference" },
                                                            { 570490896, "LOPatientSpeciesDescription" },
                                                            { 570556432, "SQPatientSpeciesCodeSequence" },
                                                            { 570621968, "CSPatientSexNeutered" },
                                                            { 571473936, "CSAnatomicalOrientationType" },
                                                            { 579993616, "LOPatientBreedDescription" },
                                                            { 580059152, "SQPatientBreedCodeSequence" },
                                                            { 580124688, "SQBreedRegistrationSequence" },
                                                            { 580190224, "LOBreedRegistrationNumber" },
                                                            { 580255760, "SQBreedRegistryCodeSequence" },
                                                            { 580321296, "PNResponsiblePerson" },
                                                            { 580386832, "CSResponsiblePersonRole" },
                                                            { 580452368, "LOResponsibleOrganization" },
                                                            { 1073741840, "LTPatientComments" },
                                                            { 2486239248, "FLExaminedBodyThickness" },
                                                            { 1048594, "LOClinicalTrialSponsorName" },
                                                            { 2097170, "LOClinicalTrialProtocolID" },
                                                            { 2162706, "LOClinicalTrialProtocolName" },
                                                            { 3145746, "LOClinicalTrialSiteID" },
                                                            { 3211282, "LOClinicalTrialSiteName" },
                                                            { 4194322, "LOClinicalTrialSubjectID" },
                                                            { 4325394, "LOClinicalTrialSubjectReadingID" },
                                                            { 5242898, "LOClinicalTrialTimePointID" },
                                                            { 5308434, "STClinicalTrialTimePointDescription" },
                                                            { 6291474, "LOClinicalTrialCoordinatingCenterName" },
                                                            { 6422546, "CSPatientIdentityRemoved" },
                                                            { 6488082, "LODeidentificationMethod" },
                                                            { 6553618, "SQDeidentificationMethodCodeSequence" },
                                                            { 7405586, "LOClinicalTrialSeriesID" },
                                                            { 7471122, "LOClinicalTrialSeriesDescription" },
                                                            { 8454162, "LOClinicalTrialProtocolEthicsCommitteeName" },
                                                            { 8519698, "LOClinicalTrialProtocolEthicsCommitteeApprovalNumber" },
                                                            { 8585234, "SQConsentForClinicalTrialUseSequence" },
                                                            { 8650770, "CSDistributionType" },
                                                            { 8716306, "CSConsentForDistributionFlag" },
                                                            { 8781842, "DAEthicsCommitteeApprovalEffectivenessStartDate" },
                                                            { 8847378, "DAEthicsCommitteeApprovalEffectivenessEndDate" },
                                                            { 2293780, "STCADFileFormat" },
                                                            { 2359316, "STComponentReferenceSystem" },
                                                            { 2424852, "STComponentManufacturingProcedure" },
                                                            { 2621460, "STComponentManufacturer" },
                                                            { 3145748, "DSMaterialThickness" },
                                                            { 3276820, "DSMaterialPipeDiameter" },
                                                            { 3407892, "DSMaterialIsolationDiameter" },
                                                            { 4325396, "STMaterialGrade" },
                                                            { 4456468, "STMaterialPropertiesDescription" },
                                                            { 4522004, "STMaterialPropertiesFileFormatRetired" },
                                                            { 4587540, "LTMaterialNotes" },
                                                            { 5242900, "CSComponentShape" },
                                                            { 5373972, "CSCurvatureType" },
                                                            { 5505044, "DSOuterDiameter" },
                                                            { 5636116, "DSInnerDiameter" },
                                                            { 16777236, "LOComponentWelderIDs" },
                                                            { 16842772, "CSSecondaryApprovalStatus" },
                                                            { 16908308, "DASecondaryReviewDate" },
                                                            { 16973844, "TMSecondaryReviewTime" },
                                                            { 17039380, "PNSecondaryReviewerName" },
                                                            { 17104916, "STRepairID" },
                                                            { 17170452, "SQMultipleComponentApprovalSequence" },
                                                            { 17235988, "CSOtherApprovalStatus" },
                                                            { 17301524, "CSOtherSecondaryApprovalStatus" },
                                                            { 269484052, "STActualEnvironmentalConditions" },
                                                            { 270532628, "DAExpiryDate" },
                                                            { 272629780, "STEnvironmentalConditions" },
                                                            { 537002004, "SQEvaluatorSequence" },
                                                            { 537133076, "ISEvaluatorNumber" },
                                                            { 537264148, "PNEvaluatorName" },
                                                            { 537395220, "ISEvaluationAttempt" },
                                                            { 538050580, "SQIndicationSequence" },
                                                            { 538181652, "ISIndicationNumber" },
                                                            { 538312724, "SHIndicationLabel" },
                                                            { 538443796, "STIndicationDescription" },
                                                            { 538574868, "CSIndicationType" },
                                                            { 538705940, "CSIndicationDisposition" },
                                                            { 538837012, "SQIndicationROISequence" },
                                                            { 540016660, "SQIndicationPhysicalPropertySequence" },
                                                            { 540147732, "SHPropertyLabel" },
                                                            { 570556436, "ISCoordinateSystemNumberOfAxes" },
                                                            { 570687508, "SQCoordinateSystemAxesSequence" },
                                                            { 570818580, "STCoordinateSystemAxisDescription" },
                                                            { 570949652, "CSCoordinateSystemDataSetMapping" },
                                                            { 571080724, "ISCoordinateSystemAxisNumber" },
                                                            { 571211796, "CSCoordinateSystemAxisType" },
                                                            { 571342868, "CSCoordinateSystemAxisUnits" },
                                                            { 571473940, "OBCoordinateSystemAxisValues" },
                                                            { 572522516, "SQCoordinateSystemTransformSequence" },
                                                            { 572653588, "STTransformDescription" },
                                                            { 572784660, "ISTransformNumberOfAxes" },
                                                            { 572915732, "ISTransformOrderOfAxes" },
                                                            { 573046804, "CSTransformedAxisUnits" },
                                                            { 573177876, "DSCoordinateSystemTransformRotationAndScaleMatrix" },
                                                            { 573308948, "DSCoordinateSystemTransformTranslationMatrix" },
                                                            { 806420500, "DSInternalDetectorFrameTime" },
                                                            { 806486036, "DSNumberOfFramesIntegrated" },
                                                            { 807403540, "SQDetectorTemperatureSequence" },
                                                            { 807534612, "STSensorName" },
                                                            { 807665684, "DSHorizontalOffsetOfSensor" },
                                                            { 807796756, "DSVerticalOffsetOfSensor" },
                                                            { 807927828, "DSSensorTemperature" },
                                                            { 809500692, "SQDarkCurrentSequence" },
                                                            { 810549268, "OBDarkCurrentCounts" },
                                                            { 811597844, "SQGainCorrectionReferenceSequence" },
                                                            { 812646420, "OBAirCounts" },
                                                            { 812711956, "DSKVUsedInGainCalibration" },
                                                            { 812777492, "DSMAUsedInGainCalibration" },
                                                            { 812843028, "DSNumberOfFramesUsedForIntegration" },
                                                            { 812908564, "LOFilterMaterialUsedInGainCalibration" },
                                                            { 812974100, "DSFilterThicknessUsedInGainCalibration" },
                                                            { 813039636, "DADateOfGainCalibration" },
                                                            { 813105172, "TMTimeOfGainCalibration" },
                                                            { 813694996, "OBBadPixelImage" },
                                                            { 815333396, "LTCalibrationNotes" },
                                                            { 1073872916, "SQPulserEquipmentSequence" },
                                                            { 1074003988, "CSPulserType" },
                                                            { 1074135060, "LTPulserNotes" },
                                                            { 1074266132, "SQReceiverEquipmentSequence" },
                                                            { 1074397204, "CSAmplifierType" },
                                                            { 1074528276, "LTReceiverNotes" },
                                                            { 1074659348, "SQPreAmplifierEquipmentSequence" },
                                                            { 1074724884, "LTPreAmplifierNotes" },
                                                            { 1074790420, "SQTransmitTransducerSequence" },
                                                            { 1074855956, "SQReceiveTransducerSequence" },
                                                            { 1074921492, "USNumberOfElements" },
                                                            { 1074987028, "CSElementShape" },
                                                            { 1075052564, "DSElementDimensionA" },
                                                            { 1075118100, "DSElementDimensionB" },
                                                            { 1075183636, "DSElementPitchA" },
                                                            { 1075249172, "DSMeasuredBeamDimensionA" },
                                                            { 1075314708, "DSMeasuredBeamDimensionB" },
                                                            { 1075380244, "DSLocationOfMeasuredBeamDiameter" },
                                                            { 1075445780, "DSNominalFrequency" },
                                                            { 1075511316, "DSMeasuredCenterFrequency" },
                                                            { 1075576852, "DSMeasuredBandwidth" },
                                                            { 1075642388, "DSElementPitchB" },
                                                            { 1075838996, "SQPulserSettingsSequence" },
                                                            { 1075970068, "DSPulseWidth" },
                                                            { 1076101140, "DSExcitationFrequency" },
                                                            { 1076232212, "CSModulationType" },
                                                            { 1076363284, "DSDamping" },
                                                            { 1076887572, "SQReceiverSettingsSequence" },
                                                            { 1076953108, "DSAcquiredSoundpathLength" },
                                                            { 1077018644, "CSAcquisitionCompressionType" },
                                                            { 1077084180, "ISAcquisitionSampleSize" },
                                                            { 1077149716, "DSRectifierSmoothing" },
                                                            { 1077215252, "SQDACSequence" },
                                                            { 1077280788, "CSDACType" },
                                                            { 1077411860, "DSDACGainPoints" },
                                                            { 1077542932, "DSDACTimePoints" },
                                                            { 1077674004, "DSDACAmplitude" },
                                                            { 1077936148, "SQPreAmplifierSettingsSequence" },
                                                            { 1078984724, "SQTransmitTransducerSettingsSequence" },
                                                            { 1079050260, "SQReceiveTransducerSettingsSequence" },
                                                            { 1079115796, "DSIncidentAngle" },
                                                            { 1079246868, "STCouplingTechnique" },
                                                            { 1079377940, "STCouplingMedium" },
                                                            { 1079443476, "DSCouplingVelocity" },
                                                            { 1079509012, "DSProbeCenterLocationX" },
                                                            { 1079574548, "DSProbeCenterLocationZ" },
                                                            { 1079640084, "DSSoundPathLength" },
                                                            { 1079771156, "STDelayLawIdentifier" },
                                                            { 1080033300, "SQGateSettingsSequence" },
                                                            { 1080164372, "DSGateThreshold" },
                                                            { 1080295444, "DSVelocityOfSound" },
                                                            { 1081081876, "SQCalibrationSettingsSequence" },
                                                            { 1081212948, "STCalibrationProcedure" },
                                                            { 1081344020, "SHProcedureVersion" },
                                                            { 1081475092, "DAProcedureCreationDate" },
                                                            { 1081606164, "DAProcedureExpirationDate" },
                                                            { 1081737236, "DAProcedureLastModifiedDate" },
                                                            { 1081868308, "TMCalibrationTime" },
                                                            { 1081999380, "DACalibrationDate" },
                                                            { 1082130452, "SQProbeDriveEquipmentSequence" },
                                                            { 1082195988, "CSDriveType" },
                                                            { 1082261524, "LTProbeDriveNotes" },
                                                            { 1082327060, "SQDriveProbeSequence" },
                                                            { 1082392596, "DSProbeInductance" },
                                                            { 1082458132, "DSProbeResistance" },
                                                            { 1082523668, "SQReceiveProbeSequence" },
                                                            { 1082589204, "SQProbeDriveSettingsSequence" },
                                                            { 1082654740, "DSBridgeResistors" },
                                                            { 1082720276, "DSProbeOrientationAngle" },
                                                            { 1082851348, "DSUserSelectedGainY" },
                                                            { 1082916884, "DSUserSelectedPhase" },
                                                            { 1082982420, "DSUserSelectedOffsetX" },
                                                            { 1083047956, "DSUserSelectedOffsetY" },
                                                            { 1083244564, "SQChannelSettingsSequence" },
                                                            { 1083310100, "DSChannelThreshold" },
                                                            { 1083834388, "SQScannerSettingsSequence" },
                                                            { 1083899924, "STScanProcedure" },
                                                            { 1083965460, "DSTranslationRateX" },
                                                            { 1084030996, "DSTranslationRateY" },
                                                            { 1084162068, "DSChannelOverlap" },
                                                            { 1084227604, "LOImageQualityIndicatorType" },
                                                            { 1084293140, "LOImageQualityIndicatorMaterial" },
                                                            { 1084358676, "LOImageQualityIndicatorSize" },
                                                            { 1342308372, "ISLINACEnergy" },
                                                            { 1342439444, "ISLINACOutput" },
                                                            { 1358954516, "USActiveAperture" },
                                                            { 1359020052, "DSTotalAperture" },
                                                            { 1359085588, "DSApertureElevation" },
                                                            { 1359151124, "DSMainLobeAngle" },
                                                            { 1359216660, "DSMainRoofAngle" },
                                                            { 1359282196, "CSConnectorType" },
                                                            { 1359347732, "SHWedgeModelNumber" },
                                                            { 1359413268, "DSWedgeAngleFloat" },
                                                            { 1359478804, "DSWedgeRoofAngle" },
                                                            { 1359544340, "CSWedgeElement1Position" },
                                                            { 1359609876, "DSWedgeMaterialVelocity" },
                                                            { 1359675412, "SHWedgeMaterial" },
                                                            { 1359740948, "DSWedgeOffsetZ" },
                                                            { 1359806484, "DSWedgeOriginOffsetX" },
                                                            { 1359872020, "DSWedgeTimeDelay" },
                                                            { 1359937556, "SHWedgeName" },
                                                            { 1360003092, "SHWedgeManufacturerName" },
                                                            { 1360068628, "LOWedgeDescription" },
                                                            { 1360134164, "DSNominalBeamAngle" },
                                                            { 1360199700, "DSWedgeOffsetX" },
                                                            { 1360265236, "DSWedgeOffsetY" },
                                                            { 1360330772, "DSWedgeTotalLength" },
                                                            { 1360396308, "DSWedgeInContactLength" },
                                                            { 1360461844, "DSWedgeFrontGap" },
                                                            { 1360527380, "DSWedgeTotalHeight" },
                                                            { 1360592916, "DSWedgeFrontHeight" },
                                                            { 1360658452, "DSWedgeRearHeight" },
                                                            { 1360723988, "DSWedgeTotalWidth" },
                                                            { 1360789524, "DSWedgeInContactWidth" },
                                                            { 1360855060, "DSWedgeChamferHeight" },
                                                            { 1360920596, "CSWedgeCurve" },
                                                            { 1360986132, "DSRadiusAlongWedge" },
                                                            { 1048600, "LOContrastBolusAgent" },
                                                            { 1179672, "SQContrastBolusAgentSequence" },
                                                            { 1245208, "FLContrastBolusT1Relaxivity" },
                                                            { 1310744, "SQContrastBolusAdministrationRouteSequence" },
                                                            { 1376280, "CSBodyPartExamined" },
                                                            { 2097176, "CSScanningSequence" },
                                                            { 2162712, "CSSequenceVariant" },
                                                            { 2228248, "CSScanOptions" },
                                                            { 2293784, "CSMRAcquisitionType" },
                                                            { 2359320, "SHSequenceName" },
                                                            { 2424856, "CSAngioFlag" },
                                                            { 2490392, "SQInterventionDrugInformationSequence" },
                                                            { 2555928, "TMInterventionDrugStopTime" },
                                                            { 2621464, "DSInterventionDrugDose" },
                                                            { 2687000, "SQInterventionDrugCodeSequence" },
                                                            { 2752536, "SQAdditionalDrugSequence" },
                                                            { 3145752, "LORadionuclide" },
                                                            { 3211288, "LORadiopharmaceutical" },
                                                            { 3276824, "DSEnergyWindowCenterline" },
                                                            { 3342360, "DSEnergyWindowTotalWidth" },
                                                            { 3407896, "LOInterventionDrugName" },
                                                            { 3473432, "TMInterventionDrugStartTime" },
                                                            { 3538968, "SQInterventionSequence" },
                                                            { 3604504, "CSTherapyType" },
                                                            { 3670040, "CSInterventionStatus" },
                                                            { 3735576, "CSTherapyDescription" },
                                                            { 3801112, "STInterventionDescription" },
                                                            { 4194328, "ISCineRate" },
                                                            { 4325400, "CSInitialCineRunState" },
                                                            { 5242904, "DSSliceThickness" },
                                                            { 6291480, "DSKVP" },
                                                            { 7340056, "ISCountsAccumulated" },
                                                            { 7405592, "CSAcquisitionTerminationCondition" },
                                                            { 7471128, "DSEffectiveDuration" },
                                                            { 7536664, "CSAcquisitionStartCondition" },
                                                            { 7602200, "ISAcquisitionStartConditionData" },
                                                            { 7667736, "ISAcquisitionTerminationConditionData" },
                                                            { 8388632, "DSRepetitionTime" },
                                                            { 8454168, "DSEchoTime" },
                                                            { 8519704, "DSInversionTime" },
                                                            { 8585240, "DSNumberOfAverages" },
                                                            { 8650776, "DSImagingFrequency" },
                                                            { 8716312, "SHImagedNucleus" },
                                                            { 8781848, "ISEchoNumbers" },
                                                            { 8847384, "DSMagneticFieldStrength" },
                                                            { 8912920, "DSSpacingBetweenSlices" },
                                                            { 8978456, "ISNumberOfPhaseEncodingSteps" },
                                                            { 9437208, "DSDataCollectionDiameter" },
                                                            { 9502744, "ISEchoTrainLength" },
                                                            { 9633816, "DSPercentSampling" },
                                                            { 9699352, "DSPercentPhaseFieldOfView" },
                                                            { 9764888, "DSPixelBandwidth" },
                                                            { 268435480, "LODeviceSerialNumber" },
                                                            { 268566552, "UIDeviceUID" },
                                                            { 268632088, "LODeviceID" },
                                                            { 268697624, "LOPlateID" },
                                                            { 268763160, "LOGeneratorID" },
                                                            { 268828696, "LOGridID" },
                                                            { 268894232, "LOCassetteID" },
                                                            { 268959768, "LOGantryID" },
                                                            { 269025304, "UTUniqueDeviceIdentifier" },
                                                            { 269090840, "SQUDISequence" },
                                                            { 269484056, "LOSecondaryCaptureDeviceID" },
                                                            { 269549592, "LOHardcopyCreationDeviceID" },
                                                            { 269615128, "DADateOfSecondaryCapture" },
                                                            { 269746200, "TMTimeOfSecondaryCapture" },
                                                            { 269877272, "LOSecondaryCaptureDeviceManufacturer" },
                                                            { 269942808, "LOHardcopyDeviceManufacturer" },
                                                            { 270008344, "LOSecondaryCaptureDeviceManufacturerModelName" },
                                                            { 270073880, "LOSecondaryCaptureDeviceSoftwareVersions" },
                                                            { 270139416, "LOHardcopyDeviceSoftwareVersion" },
                                                            { 270204952, "LOHardcopyDeviceManufacturerModelName" },
                                                            { 270532632, "LOSoftwareVersions" },
                                                            { 270663704, "SHVideoImageFormatAcquired" },
                                                            { 270729240, "LODigitalImageFormatAcquired" },
                                                            { 271581208, "LOProtocolName" },
                                                            { 272629784, "LOContrastBolusRoute" },
                                                            { 272695320, "DSContrastBolusVolume" },
                                                            { 272760856, "TMContrastBolusStartTime" },
                                                            { 272826392, "TMContrastBolusStopTime" },
                                                            { 272891928, "DSContrastBolusTotalDose" },
                                                            { 272957464, "ISSyringeCounts" },
                                                            { 273023000, "DSContrastFlowRate" },
                                                            { 273088536, "DSContrastFlowDuration" },
                                                            { 273154072, "CSContrastBolusIngredient" },
                                                            { 273219608, "DSContrastBolusIngredientConcentration" },
                                                            { 273678360, "DSSpatialResolution" },
                                                            { 274726936, "DSTriggerTime" },
                                                            { 274792472, "LOTriggerSourceOrType" },
                                                            { 274858008, "ISNominalInterval" },
                                                            { 274923544, "DSFrameTime" },
                                                            { 274989080, "LOCardiacFramingType" },
                                                            { 275054616, "DSFrameTimeVector" },
                                                            { 275120152, "DSFrameDelay" },
                                                            { 275185688, "DSImageTriggerDelay" },
                                                            { 275251224, "DSMultiplexGroupTimeOffset" },
                                                            { 275316760, "DSTriggerTimeOffset" },
                                                            { 275382296, "CSSynchronizationTrigger" },
                                                            { 275513368, "USSynchronizationChannel" },
                                                            { 275644440, "ULTriggerSamplePosition" },
                                                            { 275775512, "LORadiopharmaceuticalRoute" },
                                                            { 275841048, "DSRadiopharmaceuticalVolume" },
                                                            { 275906584, "TMRadiopharmaceuticalStartTime" },
                                                            { 275972120, "TMRadiopharmaceuticalStopTime" },
                                                            { 276037656, "DSRadionuclideTotalDose" },
                                                            { 276103192, "DSRadionuclideHalfLife" },
                                                            { 276168728, "DSRadionuclidePositronFraction" },
                                                            { 276234264, "DSRadiopharmaceuticalSpecificActivity" },
                                                            { 276299800, "DTRadiopharmaceuticalStartDateTime" },
                                                            { 276365336, "DTRadiopharmaceuticalStopDateTime" },
                                                            { 276824088, "CSBeatRejectionFlag" },
                                                            { 276889624, "ISLowRRValue" },
                                                            { 276955160, "ISHighRRValue" },
                                                            { 277020696, "ISIntervalsAcquired" },
                                                            { 277086232, "ISIntervalsRejected" },
                                                            { 277151768, "LOPVCRejection" },
                                                            { 277217304, "ISSkipBeats" },
                                                            { 277348376, "ISHeartRate" },
                                                            { 277872664, "ISCardiacNumberOfImages" },
                                                            { 278134808, "ISTriggerWindow" },
                                                            { 285212696, "DSReconstructionDiameter" },
                                                            { 286261272, "DSDistanceSourceToDetector" },
                                                            { 286326808, "DSDistanceSourceToPatient" },
                                                            { 286523416, "DSEstimatedRadiographicMagnificationFactor" },
                                                            { 287309848, "DSGantryDetectorTilt" },
                                                            { 287375384, "DSGantryDetectorSlew" },
                                                            { 288358424, "DSTableHeight" },
                                                            { 288423960, "DSTableTraverse" },
                                                            { 288620568, "CSTableMotion" },
                                                            { 288686104, "DSTableVerticalIncrement" },
                                                            { 288751640, "DSTableLateralIncrement" },
                                                            { 288817176, "DSTableLongitudinalIncrement" },
                                                            { 288882712, "DSTableAngle" },
                                                            { 289013784, "CSTableType" },
                                                            { 289407000, "CSRotationDirection" },
                                                            { 289472536, "DSAngularPosition" },
                                                            { 289538072, "DSRadialPosition" },
                                                            { 289603608, "DSScanArc" },
                                                            { 289669144, "DSAngularStep" },
                                                            { 289734680, "DSCenterOfRotationOffset" },
                                                            { 289800216, "DSRotationOffset" },
                                                            { 289865752, "CSFieldOfViewShape" },
                                                            { 289996824, "ISFieldOfViewDimensions" },
                                                            { 290455576, "ISExposureTime" },
                                                            { 290521112, "ISXRayTubeCurrent" },
                                                            { 290586648, "ISExposure" },
                                                            { 290652184, "ISExposureInuAs" },
                                                            { 290717720, "DSAveragePulseWidth" },
                                                            { 290783256, "CSRadiationSetting" },
                                                            { 290848792, "CSRectificationType" },
                                                            { 291110936, "CSRadiationMode" },
                                                            { 291373080, "DSImageAndFluoroscopyAreaDoseProduct" },
                                                            { 291504152, "SHFilterType" },
                                                            { 291569688, "LOTypeOfFilters" },
                                                            { 291635224, "DSIntensifierSize" },
                                                            { 291766296, "DSImagerPixelSpacing" },
                                                            { 291897368, "CSGrid" },
                                                            { 292552728, "ISGeneratorPower" },
                                                            { 293601304, "SHCollimatorGridName" },
                                                            { 293666840, "CSCollimatorType" },
                                                            { 293732376, "ISFocalDistance" },
                                                            { 293797912, "DSXFocusCenter" },
                                                            { 293863448, "DSYFocusCenter" },
                                                            { 294649880, "DSFocalSpots" },
                                                            { 294715416, "CSAnodeTargetMaterial" },
                                                            { 295698456, "DSBodyPartThickness" },
                                                            { 295829528, "DSCompressionForce" },
                                                            { 295960600, "LOPaddleDescription" },
                                                            { 301989912, "DADateOfLastCalibration" },
                                                            { 302055448, "TMTimeOfLastCalibration" },
                                                            { 302120984, "DTDateTimeOfLastCalibration" },
                                                            { 303038488, "SHConvolutionKernel" },
                                                            { 306184216, "ISUpperLowerPixelValues" },
                                                            { 306315288, "ISActualFrameDuration" },
                                                            { 306380824, "ISCountRate" },
                                                            { 306446360, "USPreferredPlaybackSequencing" },
                                                            { 307232792, "SHReceiveCoilName" },
                                                            { 307298328, "SHTransmitCoilName" },
                                                            { 308281368, "SHPlateType" },
                                                            { 308346904, "LOPhosphorType" },
                                                            { 309395480, "FDWaterEquivalentDiameter" },
                                                            { 309461016, "SQWaterEquivalentDiameterCalculationMethodCodeSequence" },
                                                            { 318767128, "DSScanVelocity" },
                                                            { 318832664, "CSWholeBodyTechnique" },
                                                            { 318898200, "ISScanLength" },
                                                            { 319815704, "USAcquisitionMatrix" },
                                                            { 319946776, "CSInPlanePhaseEncodingDirection" },
                                                            { 320077848, "DSFlipAngle" },
                                                            { 320143384, "CSVariableFlipAngleFlag" },
                                                            { 320208920, "DSSAR" },
                                                            { 320339992, "DSdBdt" },
                                                            { 320864280, "FLB1rms" },
                                                            { 335544344, "LOAcquisitionDeviceProcessingDescription" },
                                                            { 335609880, "LOAcquisitionDeviceProcessingCode" },
                                                            { 335675416, "CSCassetteOrientation" },
                                                            { 335740952, "CSCassetteSize" },
                                                            { 335806488, "USExposuresOnPlate" },
                                                            { 335872024, "ISRelativeXRayExposure" },
                                                            { 336658456, "DSExposureIndex" },
                                                            { 336723992, "DSTargetExposureIndex" },
                                                            { 336789528, "DSDeviationIndex" },
                                                            { 340787224, "DSColumnAngulation" },
                                                            { 341835800, "DSTomoLayerHeight" },
                                                            { 342884376, "DSTomoAngle" },
                                                            { 343932952, "DSTomoTime" },
                                                            { 344981528, "CSTomoType" },
                                                            { 345047064, "CSTomoClass" },
                                                            { 345309208, "ISNumberOfTomosynthesisSourceImages" },
                                                            { 352321560, "CSPositionerMotion" },
                                                            { 352845848, "CSPositionerType" },
                                                            { 353370136, "DSPositionerPrimaryAngle" },
                                                            { 353435672, "DSPositionerSecondaryAngle" },
                                                            { 354418712, "DSPositionerPrimaryAngleIncrement" },
                                                            { 354484248, "DSPositionerSecondaryAngleIncrement" },
                                                            { 355467288, "DSDetectorPrimaryAngle" },
                                                            { 355532824, "DSDetectorSecondaryAngle" },
                                                            { 369098776, "CSShutterShape" },
                                                            { 369229848, "ISShutterLeftVerticalEdge" },
                                                            { 369360920, "ISShutterRightVerticalEdge" },
                                                            { 369491992, "ISShutterUpperHorizontalEdge" },
                                                            { 369623064, "ISShutterLowerHorizontalEdge" },
                                                            { 370147352, "ISCenterOfCircularShutter" },
                                                            { 370278424, "ISRadiusOfCircularShutter" },
                                                            { 371195928, "ISVerticesOfThePolygonalShutter" },
                                                            { 371327000, "USShutterPresentationValue" },
                                                            { 371392536, "USShutterOverlayGroup" },
                                                            { 371458072, "USShutterPresentationColorCIELabValue" },
                                                            { 385875992, "CSCollimatorShape" },
                                                            { 386007064, "ISCollimatorLeftVerticalEdge" },
                                                            { 386138136, "ISCollimatorRightVerticalEdge" },
                                                            { 386269208, "ISCollimatorUpperHorizontalEdge" },
                                                            { 386400280, "ISCollimatorLowerHorizontalEdge" },
                                                            { 386924568, "ISCenterOfCircularCollimator" },
                                                            { 387055640, "ISRadiusOfCircularCollimator" },
                                                            { 387973144, "ISVerticesOfThePolygonalCollimator" },
                                                            { 402653208, "CSAcquisitionTimeSynchronized" },
                                                            { 402718744, "SHTimeSource" },
                                                            { 402784280, "CSTimeDistributionProtocol" },
                                                            { 402849816, "LONTPSourceAddress" },
                                                            { 536936472, "ISPageNumberVector" },
                                                            { 537002008, "SHFrameLabelVector" },
                                                            { 537067544, "DSFramePrimaryAngleVector" },
                                                            { 537133080, "DSFrameSecondaryAngleVector" },
                                                            { 537198616, "DSSliceLocationVector" },
                                                            { 537264152, "SHDisplayWindowLabelVector" },
                                                            { 537919512, "DSNominalScannedPixelSpacing" },
                                                            { 538968088, "CSDigitizingDeviceTransportDirection" },
                                                            { 540016664, "DSRotationOfScannedFilm" },
                                                            { 541130776, "SQBiopsyTargetSequence" },
                                                            { 541196312, "UITargetUID" },
                                                            { 541261848, "FLLocalizingCursorPosition" },
                                                            { 541327384, "FLCalculatedTargetPosition" },
                                                            { 541392920, "SHTargetLabel" },
                                                            { 541458456, "FLDisplayedZValue" },
                                                            { 822083608, "CSIVUSAcquisition" },
                                                            { 822149144, "DSIVUSPullbackRate" },
                                                            { 822214680, "DSIVUSGatedRate" },
                                                            { 822280216, "ISIVUSPullbackStartFrameNumber" },
                                                            { 822345752, "ISIVUSPullbackStopFrameNumber" },
                                                            { 822411288, "ISLesionNumber" },
                                                            { 1073741848, "LTAcquisitionComments" },
                                                            { 1342177304, "SHOutputPower" },
                                                            { 1343225880, "LOTransducerData" },
                                                            { 1343356952, "DSFocusDepth" },
                                                            { 1344274456, "LOProcessingFunction" },
                                                            { 1344339992, "LOPostprocessingFunction" },
                                                            { 1344405528, "DSMechanicalIndex" },
                                                            { 1344536600, "DSBoneThermalIndex" },
                                                            { 1344667672, "DSCranialThermalIndex" },
                                                            { 1344733208, "DSSoftTissueThermalIndex" },
                                                            { 1344798744, "DSSoftTissueFocusThermalIndex" },
                                                            { 1344864280, "DSSoftTissueSurfaceThermalIndex" },
                                                            { 1345323032, "DSDynamicRange" },
                                                            { 1346371608, "DSTotalGain" },
                                                            { 1347420184, "ISDepthOfScanField" },
                                                            { 1358954520, "CSPatientPosition" },
                                                            { 1359020056, "CSViewPosition" },
                                                            { 1359216664, "SQProjectionEponymousNameCodeSequence" },
                                                            { 1376780312, "DSImageTransformationMatrix" },
                                                            { 1376911384, "DSImageTranslationVector" },
                                                            { 1610612760, "DSSensitivity" },
                                                            { 1611726872, "SQSequenceOfUltrasoundRegions" },
                                                            { 1611792408, "USRegionSpatialFormat" },
                                                            { 1611923480, "USRegionDataType" },
                                                            { 1612054552, "ULRegionFlags" },
                                                            { 1612185624, "ULRegionLocationMinX0" },
                                                            { 1612316696, "ULRegionLocationMinY0" },
                                                            { 1612447768, "ULRegionLocationMaxX1" },
                                                            { 1612578840, "ULRegionLocationMaxY1" },
                                                            { 1612709912, "SLReferencePixelX0" },
                                                            { 1612840984, "SLReferencePixelY0" },
                                                            { 1612972056, "USPhysicalUnitsXDirection" },
                                                            { 1613103128, "USPhysicalUnitsYDirection" },
                                                            { 1613234200, "FDReferencePixelPhysicalValueX" },
                                                            { 1613365272, "FDReferencePixelPhysicalValueY" },
                                                            { 1613496344, "FDPhysicalDeltaX" },
                                                            { 1613627416, "FDPhysicalDeltaY" },
                                                            { 1613758488, "ULTransducerFrequency" },
                                                            { 1613824024, "CSTransducerType" },
                                                            { 1613889560, "ULPulseRepetitionFrequency" },
                                                            { 1614020632, "FDDopplerCorrectionAngle" },
                                                            { 1614151704, "FDSteeringAngle" },
                                                            { 1614282776, "ULDopplerSampleVolumeXPositionRetired" },
                                                            { 1614348312, "SLDopplerSampleVolumeXPosition" },
                                                            { 1614413848, "ULDopplerSampleVolumeYPositionRetired" },
                                                            { 1614479384, "SLDopplerSampleVolumeYPosition" },
                                                            { 1614544920, "ULTMLinePositionX0Retired" },
                                                            { 1614610456, "SLTMLinePositionX0" },
                                                            { 1614675992, "ULTMLinePositionY0Retired" },
                                                            { 1614741528, "SLTMLinePositionY0" },
                                                            { 1614807064, "ULTMLinePositionX1Retired" },
                                                            { 1614872600, "SLTMLinePositionX1" },
                                                            { 1614938136, "ULTMLinePositionY1Retired" },
                                                            { 1615003672, "SLTMLinePositionY1" },
                                                            { 1615069208, "USPixelComponentOrganization" },
                                                            { 1615200280, "ULPixelComponentMask" },
                                                            { 1615331352, "ULPixelComponentRangeStart" },
                                                            { 1615462424, "ULPixelComponentRangeStop" },
                                                            { 1615593496, "USPixelComponentPhysicalUnits" },
                                                            { 1615724568, "USPixelComponentDataType" },
                                                            { 1615855640, "ULNumberOfTableBreakPoints" },
                                                            { 1615986712, "ULTableOfXBreakPoints" },
                                                            { 1616117784, "FDTableOfYBreakPoints" },
                                                            { 1616248856, "ULNumberOfTableEntries" },
                                                            { 1616379928, "ULTableOfPixelValues" },
                                                            { 1616511000, "FLTableOfParameterValues" },
                                                            { 1616904216, "FLRWaveTimeVector" },
                                                            { 1879048216, "CSDetectorConditionsNominalFlag" },
                                                            { 1879113752, "DSDetectorTemperature" },
                                                            { 1879310360, "CSDetectorType" },
                                                            { 1879375896, "CSDetectorConfiguration" },
                                                            { 1879441432, "LTDetectorDescription" },
                                                            { 1879572504, "LTDetectorMode" },
                                                            { 1879703576, "SHDetectorID" },
                                                            { 1879834648, "DADateOfLastDetectorCalibration" },
                                                            { 1879965720, "TMTimeOfLastDetectorCalibration" },
                                                            { 1880096792, "ISExposuresOnDetectorSinceLastCalibration" },
                                                            { 1880162328, "ISExposuresOnDetectorSinceManufactured" },
                                                            { 1880227864, "DSDetectorTimeSinceLastExposure" },
                                                            { 1880358936, "DSDetectorActiveTime" },
                                                            { 1880490008, "DSDetectorActivationOffsetFromExposure" },
                                                            { 1880752152, "DSDetectorBinning" },
                                                            { 1881145368, "DSDetectorElementPhysicalSize" },
                                                            { 1881276440, "DSDetectorElementSpacing" },
                                                            { 1881407512, "CSDetectorActiveShape" },
                                                            { 1881538584, "DSDetectorActiveDimensions" },
                                                            { 1881669656, "DSDetectorActiveOrigin" },
                                                            { 1881800728, "LODetectorManufacturerName" },
                                                            { 1881866264, "LODetectorManufacturerModelName" },
                                                            { 1882193944, "DSFieldOfViewOrigin" },
                                                            { 1882325016, "DSFieldOfViewRotation" },
                                                            { 1882456088, "CSFieldOfViewHorizontalFlip" },
                                                            { 1882587160, "FLPixelDataAreaOriginRelativeToFOV" },
                                                            { 1882718232, "FLPixelDataAreaRotationAngleRelativeToFOV" },
                                                            { 1883242520, "LTGridAbsorbingMaterial" },
                                                            { 1883308056, "LTGridSpacingMaterial" },
                                                            { 1883373592, "DSGridThickness" },
                                                            { 1883504664, "DSGridPitch" },
                                                            { 1883635736, "ISGridAspectRatio" },
                                                            { 1883766808, "DSGridPeriod" },
                                                            { 1884028952, "DSGridFocalDistance" },
                                                            { 1884291096, "CSFilterMaterial" },
                                                            { 1884422168, "DSFilterThicknessMinimum" },
                                                            { 1884553240, "DSFilterThicknessMaximum" },
                                                            { 1884684312, "FLFilterBeamPathLengthMinimum" },
                                                            { 1884815384, "FLFilterBeamPathLengthMaximum" },
                                                            { 1885339672, "CSExposureControlMode" },
                                                            { 1885470744, "LTExposureControlModeDescription" },
                                                            { 1885601816, "CSExposureStatus" },
                                                            { 1885667352, "DSPhototimerSetting" },
                                                            { 2169503768, "DSExposureTimeInuS" },
                                                            { 2169569304, "DSXRayTubeCurrentInuA" },
                                                            { 2416181272, "CSContentQualification" },
                                                            { 2416246808, "SHPulseSequenceName" },
                                                            { 2416312344, "SQMRImagingModifierSequence" },
                                                            { 2416443416, "CSEchoPulseSequence" },
                                                            { 2416508952, "CSInversionRecovery" },
                                                            { 2416967704, "CSFlowCompensation" },
                                                            { 2417033240, "CSMultipleSpinEcho" },
                                                            { 2417098776, "CSMultiPlanarExcitation" },
                                                            { 2417229848, "CSPhaseContrast" },
                                                            { 2417295384, "CSTimeOfFlightContrast" },
                                                            { 2417360920, "CSSpoiling" },
                                                            { 2417426456, "CSSteadyStatePulseSequence" },
                                                            { 2417491992, "CSEchoPlanarPulseSequence" },
                                                            { 2417557528, "FDTagAngleFirstAxis" },
                                                            { 2418016280, "CSMagnetizationTransfer" },
                                                            { 2418081816, "CST2Preparation" },
                                                            { 2418147352, "CSBloodSignalNulling" },
                                                            { 2418278424, "CSSaturationRecovery" },
                                                            { 2418343960, "CSSpectrallySelectedSuppression" },
                                                            { 2418409496, "CSSpectrallySelectedExcitation" },
                                                            { 2418475032, "CSSpatialPresaturation" },
                                                            { 2418540568, "CSTagging" },
                                                            { 2418606104, "CSOversamplingPhase" },
                                                            { 2419064856, "FDTagSpacingFirstDimension" },
                                                            { 2419195928, "CSGeometryOfKSpaceTraversal" },
                                                            { 2419261464, "CSSegmentedKSpaceTraversal" },
                                                            { 2419327000, "CSRectilinearPhaseEncodeReordering" },
                                                            { 2419392536, "FDTagThickness" },
                                                            { 2419458072, "CSPartialFourierDirection" },
                                                            { 2419523608, "CSCardiacSynchronizationTechnique" },
                                                            { 2420178968, "LOReceiveCoilManufacturerName" },
                                                            { 2420244504, "SQMRReceiveCoilSequence" },
                                                            { 2420310040, "CSReceiveCoilType" },
                                                            { 2420375576, "CSQuadratureReceiveCoil" },
                                                            { 2420441112, "SQMultiCoilDefinitionSequence" },
                                                            { 2420506648, "LOMultiCoilConfiguration" },
                                                            { 2420572184, "SHMultiCoilElementName" },
                                                            { 2420637720, "CSMultiCoilElementUsed" },
                                                            { 2420703256, "SQMRTransmitCoilSequence" },
                                                            { 2421162008, "LOTransmitCoilManufacturerName" },
                                                            { 2421227544, "CSTransmitCoilType" },
                                                            { 2421293080, "FDSpectralWidth" },
                                                            { 2421358616, "FDChemicalShiftReference" },
                                                            { 2421424152, "CSVolumeLocalizationTechnique" },
                                                            { 2421686296, "USMRAcquisitionFrequencyEncodingSteps" },
                                                            { 2421751832, "CSDecoupling" },
                                                            { 2422210584, "CSDecoupledNucleus" },
                                                            { 2422276120, "FDDecouplingFrequency" },
                                                            { 2422341656, "CSDecouplingMethod" },
                                                            { 2422407192, "FDDecouplingChemicalShiftReference" },
                                                            { 2422472728, "CSKSpaceFiltering" },
                                                            { 2422538264, "CSTimeDomainFiltering" },
                                                            { 2422603800, "USNumberOfZeroFills" },
                                                            { 2422669336, "CSBaselineCorrection" },
                                                            { 2422800408, "FDParallelReductionFactorInPlane" },
                                                            { 2423259160, "FDCardiacRRIntervalSpecified" },
                                                            { 2423455768, "FDAcquisitionDuration" },
                                                            { 2423521304, "DTFrameAcquisitionDateTime" },
                                                            { 2423586840, "CSDiffusionDirectionality" },
                                                            { 2423652376, "SQDiffusionGradientDirectionSequence" },
                                                            { 2423717912, "CSParallelAcquisition" },
                                                            { 2423783448, "CSParallelAcquisitionTechnique" },
                                                            { 2423848984, "FDInversionTimes" },
                                                            { 2424307736, "STMetaboliteMapDescription" },
                                                            { 2424373272, "CSPartialFourier" },
                                                            { 2424438808, "FDEffectiveEchoTime" },
                                                            { 2424504344, "SQMetaboliteMapCodeSequence" },
                                                            { 2424569880, "SQChemicalShiftSequence" },
                                                            { 2424635416, "CSCardiacSignalSource" },
                                                            { 2424766488, "FDDiffusionBValue" },
                                                            { 2424897560, "FDDiffusionGradientOrientation" },
                                                            { 2425356312, "FDVelocityEncodingDirection" },
                                                            { 2425421848, "FDVelocityEncodingMinimumValue" },
                                                            { 2425487384, "SQVelocityEncodingAcquisitionSequence" },
                                                            { 2425552920, "USNumberOfKSpaceTrajectories" },
                                                            { 2425618456, "CSCoverageOfKSpace" },
                                                            { 2425683992, "ULSpectroscopyAcquisitionPhaseRows" },
                                                            { 2425749528, "FDParallelReductionFactorInPlaneRetired" },
                                                            { 2425880600, "FDTransmitterFrequency" },
                                                            { 2432696344, "CSResonantNucleus" },
                                                            { 2432761880, "CSFrequencyCorrection" },
                                                            { 2432892952, "SQMRSpectroscopyFOVGeometrySequence" },
                                                            { 2432958488, "FDSlabThickness" },
                                                            { 2433024024, "FDSlabOrientation" },
                                                            { 2433089560, "FDMidSlabPosition" },
                                                            { 2433155096, "SQMRSpatialSaturationSequence" },
                                                            { 2433875992, "SQMRTimingAndRelatedParametersSequence" },
                                                            { 2434007064, "SQMREchoSequence" },
                                                            { 2434072600, "SQMRModifierSequence" },
                                                            { 2434203672, "SQMRDiffusionSequence" },
                                                            { 2434269208, "SQCardiacSynchronizationSequence" },
                                                            { 2434334744, "SQMRAveragesSequence" },
                                                            { 2435121176, "SQMRFOVGeometrySequence" },
                                                            { 2435186712, "SQVolumeLocalizationSequence" },
                                                            { 2435252248, "ULSpectroscopyAcquisitionDataColumns" },
                                                            { 2437349400, "CSDiffusionAnisotropyType" },
                                                            { 2438004760, "DTFrameReferenceDateTime" },
                                                            { 2438070296, "SQMRMetaboliteMapSequence" },
                                                            { 2438266904, "FDParallelReductionFactorOutOfPlane" },
                                                            { 2438529048, "ULSpectroscopyAcquisitionOutOfPlanePhaseSteps" },
                                                            { 2439381016, "CSBulkMotionStatus" },
                                                            { 2439512088, "FDParallelReductionFactorSecondInPlane" },
                                                            { 2439577624, "CSCardiacBeatRejectionTechnique" },
                                                            { 2440036376, "CSRespiratoryMotionCompensationTechnique" },
                                                            { 2440101912, "CSRespiratorySignalSource" },
                                                            { 2440167448, "CSBulkMotionCompensationTechnique" },
                                                            { 2440232984, "CSBulkMotionSignalSource" },
                                                            { 2440298520, "CSApplicableSafetyStandardAgency" },
                                                            { 2440364056, "LOApplicableSafetyStandardDescription" },
                                                            { 2440429592, "SQOperatingModeSequence" },
                                                            { 2440495128, "CSOperatingModeType" },
                                                            { 2440560664, "CSOperatingMode" },
                                                            { 2440626200, "CSSpecificAbsorptionRateDefinition" },
                                                            { 2441084952, "CSGradientOutputType" },
                                                            { 2441150488, "FDSpecificAbsorptionRateValue" },
                                                            { 2441216024, "FDGradientOutput" },
                                                            { 2441281560, "CSFlowCompensationDirection" },
                                                            { 2441347096, "FDTaggingDelay" },
                                                            { 2441412632, "STRespiratoryMotionCompensationTechniqueDescription" },
                                                            { 2441478168, "SHRespiratorySignalSourceID" },
                                                            { 2442461208, "FDChemicalShiftMinimumIntegrationLimitInHz" },
                                                            { 2442526744, "FDChemicalShiftMaximumIntegrationLimitInHz" },
                                                            { 2442592280, "SQMRVelocityEncodingSequence" },
                                                            { 2442657816, "CSFirstOrderPhaseCorrection" },
                                                            { 2442723352, "CSWaterReferencedPhaseCorrection" },
                                                            { 2449473560, "CSMRSpectroscopyAcquisitionType" },
                                                            { 2450784280, "CSRespiratoryCyclePosition" },
                                                            { 2450980888, "FDVelocityEncodingMaximumValue" },
                                                            { 2451046424, "FDTagSpacingSecondDimension" },
                                                            { 2451111960, "SSTagAngleSecondAxis" },
                                                            { 2451570712, "FDFrameAcquisitionDuration" },
                                                            { 2451963928, "SQMRImageFrameTypeSequence" },
                                                            { 2452029464, "SQMRSpectroscopyFrameTypeSequence" },
                                                            { 2452684824, "USMRAcquisitionPhaseEncodingStepsInPlane" },
                                                            { 2452750360, "USMRAcquisitionPhaseEncodingStepsOutOfPlane" },
                                                            { 2452881432, "ULSpectroscopyAcquisitionPhaseColumns" },
                                                            { 2453012504, "CSCardiacCyclePosition" },
                                                            { 2453209112, "SQSpecificAbsorptionRateSequence" },
                                                            { 2453667864, "USRFEchoTrainLength" },
                                                            { 2453733400, "USGradientEchoTrainLength" },
                                                            { 2454716440, "CSArterialSpinLabelingContrast" },
                                                            { 2454781976, "SQMRArterialSpinLabelingSequence" },
                                                            { 2454847512, "LOASLTechniqueDescription" },
                                                            { 2454913048, "USASLSlabNumber" },
                                                            { 2454978584, "FDASLSlabThickness" },
                                                            { 2455044120, "FDASLSlabOrientation" },
                                                            { 2455109656, "FDASLMidSlabPosition" },
                                                            { 2455175192, "CSASLContext" },
                                                            { 2455240728, "ULASLPulseTrainDuration" },
                                                            { 2455306264, "CSASLCrusherFlag" },
                                                            { 2455371800, "FDASLCrusherFlowLimit" },
                                                            { 2455437336, "LOASLCrusherDescription" },
                                                            { 2455502872, "CSASLBolusCutoffFlag" },
                                                            { 2455568408, "SQASLBolusCutoffTimingSequence" },
                                                            { 2455633944, "LOASLBolusCutoffTechnique" },
                                                            { 2455699480, "ULASLBolusCutoffDelayTime" },
                                                            { 2455765016, "SQASLSlabSequence" },
                                                            { 2459238424, "FDChemicalShiftMinimumIntegrationLimitInppm" },
                                                            { 2459303960, "FDChemicalShiftMaximumIntegrationLimitInppm" },
                                                            { 2459369496, "CSWaterReferenceAcquisition" },
                                                            { 2459435032, "ISEchoPeakPosition" },
                                                            { 2466316312, "SQCTAcquisitionTypeSequence" },
                                                            { 2466381848, "CSAcquisitionType" },
                                                            { 2466447384, "FDTubeAngle" },
                                                            { 2466512920, "SQCTAcquisitionDetailsSequence" },
                                                            { 2466578456, "FDRevolutionTime" },
                                                            { 2466643992, "FDSingleCollimationWidth" },
                                                            { 2466709528, "FDTotalCollimationWidth" },
                                                            { 2466775064, "SQCTTableDynamicsSequence" },
                                                            { 2466840600, "FDTableSpeed" },
                                                            { 2467299352, "FDTableFeedPerRotation" },
                                                            { 2467364888, "FDSpiralPitchFactor" },
                                                            { 2467430424, "SQCTGeometrySequence" },
                                                            { 2467495960, "FDDataCollectionCenterPatient" },
                                                            { 2467561496, "SQCTReconstructionSequence" },
                                                            { 2467627032, "CSReconstructionAlgorithm" },
                                                            { 2467692568, "CSConvolutionKernelGroup" },
                                                            { 2467758104, "FDReconstructionFieldOfView" },
                                                            { 2467823640, "FDReconstructionTargetCenterPatient" },
                                                            { 2467889176, "FDReconstructionAngle" },
                                                            { 2468347928, "SHImageFilter" },
                                                            { 2468413464, "SQCTExposureSequence" },
                                                            { 2468479000, "FDReconstructionPixelSpacing" },
                                                            { 2468544536, "CSExposureModulationType" },
                                                            { 2468610072, "FDEstimatedDoseSaving" },
                                                            { 2468675608, "SQCTXRayDetailsSequence" },
                                                            { 2468741144, "SQCTPositionSequence" },
                                                            { 2468806680, "FDTablePosition" },
                                                            { 2468872216, "FDExposureTimeInms" },
                                                            { 2468937752, "SQCTImageFrameTypeSequence" },
                                                            { 2469396504, "FDXRayTubeCurrentInmA" },
                                                            { 2469527576, "FDExposureInmAs" },
                                                            { 2469593112, "CSConstantVolumeFlag" },
                                                            { 2469658648, "CSFluoroscopyFlag" },
                                                            { 2469724184, "FDDistanceSourceToDataCollectionCenter" },
                                                            { 2469855256, "USContrastBolusAgentNumber" },
                                                            { 2469920792, "SQContrastBolusIngredientCodeSequence" },
                                                            { 2470445080, "SQContrastAdministrationProfileSequence" },
                                                            { 2470510616, "SQContrastBolusUsageSequence" },
                                                            { 2470576152, "CSContrastBolusAgentAdministered" },
                                                            { 2470641688, "CSContrastBolusAgentDetected" },
                                                            { 2470707224, "CSContrastBolusAgentPhase" },
                                                            { 2470772760, "FDCTDIvol" },
                                                            { 2470838296, "SQCTDIPhantomTypeCodeSequence" },
                                                            { 2471559192, "FLCalciumScoringMassFactorPatient" },
                                                            { 2471624728, "FLCalciumScoringMassFactorDevice" },
                                                            { 2471690264, "FLEnergyWeightingFactor" },
                                                            { 2472542232, "SQCTAdditionalXRaySourceSequence" },
                                                            { 2483093528, "SQProjectionPixelCalibrationSequence" },
                                                            { 2483159064, "FLDistanceSourceToIsocenter" },
                                                            { 2483224600, "FLDistanceObjectToTableTop" },
                                                            { 2483290136, "FLObjectPixelSpacingInCenterOfBeam" },
                                                            { 2483355672, "SQPositionerPositionSequence" },
                                                            { 2483421208, "SQTablePositionSequence" },
                                                            { 2483486744, "SQCollimatorShapeSequence" },
                                                            { 2484076568, "CSPlanesInAcquisition" },
                                                            { 2484207640, "SQXAXRFFrameCharacteristicsSequence" },
                                                            { 2484535320, "SQFrameAcquisitionSequence" },
                                                            { 2485125144, "CSXRayReceptorType" },
                                                            { 2485321752, "LOAcquisitionProtocolName" },
                                                            { 2485387288, "LTAcquisitionProtocolDescription" },
                                                            { 2485452824, "CSContrastBolusIngredientOpaque" },
                                                            { 2485518360, "FLDistanceReceptorPlaneToDetectorHousing" },
                                                            { 2485583896, "CSIntensifierActiveShape" },
                                                            { 2485649432, "FLIntensifierActiveDimensions" },
                                                            { 2485714968, "FLPhysicalDetectorSize" },
                                                            { 2486173720, "FLPositionOfIsocenterProjection" },
                                                            { 2486304792, "SQFieldOfViewSequence" },
                                                            { 2486370328, "LOFieldOfViewDescription" },
                                                            { 2486435864, "SQExposureControlSensingRegionsSequence" },
                                                            { 2486501400, "CSExposureControlSensingRegionShape" },
                                                            { 2486566936, "SSExposureControlSensingRegionLeftVerticalEdge" },
                                                            { 2486632472, "SSExposureControlSensingRegionRightVerticalEdge" },
                                                            { 2486698008, "SSExposureControlSensingRegionUpperHorizontalEdge" },
                                                            { 2486763544, "SSExposureControlSensingRegionLowerHorizontalEdge" },
                                                            { 2487222296, "SSCenterOfCircularExposureControlSensingRegion" },
                                                            { 2487287832, "USRadiusOfCircularExposureControlSensingRegion" },
                                                            { 2487353368, "SSVerticesOfThePolygonalExposureControlSensingRegion" },
                                                            { 2487681048, "FLColumnAngulationPatient" },
                                                            { 2487812120, "FLBeamAngle" },
                                                            { 2488336408, "SQFrameDetectorParametersSequence" },
                                                            { 2488401944, "FLCalculatedAnatomyThickness" },
                                                            { 2488598552, "SQCalibrationSequence" },
                                                            { 2488664088, "SQObjectThicknessSequence" },
                                                            { 2488729624, "CSPlaneIdentification" },
                                                            { 2489384984, "FLFieldOfViewDimensionsInFloat" },
                                                            { 2489450520, "SQIsocenterReferenceSystemSequence" },
                                                            { 2489516056, "FLPositionerIsocenterPrimaryAngle" },
                                                            { 2489581592, "FLPositionerIsocenterSecondaryAngle" },
                                                            { 2489647128, "FLPositionerIsocenterDetectorRotationAngle" },
                                                            { 2489712664, "FLTableXPositionToIsocenter" },
                                                            { 2489778200, "FLTableYPositionToIsocenter" },
                                                            { 2489843736, "FLTableZPositionToIsocenter" },
                                                            { 2489909272, "FLTableHorizontalRotationAngle" },
                                                            { 2490368024, "FLTableHeadTiltAngle" },
                                                            { 2490433560, "FLTableCradleTiltAngle" },
                                                            { 2490499096, "SQFrameDisplayShutterSequence" },
                                                            { 2490564632, "FLAcquiredImageAreaDoseProduct" },
                                                            { 2490630168, "CSCArmPositionerTabletopRelationship" },
                                                            { 2490761240, "SQXRayGeometrySequence" },
                                                            { 2490826776, "SQIrradiationEventIdentificationSequence" },
                                                            { 2500067352, "SQXRay3DFrameTypeSequence" },
                                                            { 2500198424, "SQContributingSourcesSequence" },
                                                            { 2500263960, "SQXRay3DAcquisitionSequence" },
                                                            { 2500329496, "FLPrimaryPositionerScanArc" },
                                                            { 2500395032, "FLSecondaryPositionerScanArc" },
                                                            { 2500853784, "FLPrimaryPositionerScanStartAngle" },
                                                            { 2500919320, "FLSecondaryPositionerScanStartAngle" },
                                                            { 2501115928, "FLPrimaryPositionerIncrement" },
                                                            { 2501181464, "FLSecondaryPositionerIncrement" },
                                                            { 2501247000, "DTStartAcquisitionDateTime" },
                                                            { 2501312536, "DTEndAcquisitionDateTime" },
                                                            { 2501378072, "SSPrimaryPositionerIncrementSign" },
                                                            { 2501443608, "SSSecondaryPositionerIncrementSign" },
                                                            { 2502164504, "LOApplicationName" },
                                                            { 2502230040, "LOApplicationVersion" },
                                                            { 2502295576, "LOApplicationManufacturer" },
                                                            { 2502361112, "CSAlgorithmType" },
                                                            { 2502426648, "LOAlgorithmDescription" },
                                                            { 2502950936, "SQXRay3DReconstructionSequence" },
                                                            { 2503016472, "LOReconstructionDescription" },
                                                            { 2503475224, "SQPerProjectionAcquisitionSequence" },
                                                            { 2504065048, "SQDetectorPositionSequence" },
                                                            { 2504130584, "SQXRayAcquisitionDoseSequence" },
                                                            { 2504196120, "FDXRaySourceIsocenterPrimaryAngle" },
                                                            { 2504261656, "FDXRaySourceIsocenterSecondaryAngle" },
                                                            { 2504327192, "FDBreastSupportIsocenterPrimaryAngle" },
                                                            { 2504392728, "FDBreastSupportIsocenterSecondaryAngle" },
                                                            { 2504458264, "FDBreastSupportXPositionToIsocenter" },
                                                            { 2504523800, "FDBreastSupportYPositionToIsocenter" },
                                                            { 2504589336, "FDBreastSupportZPositionToIsocenter" },
                                                            { 2505048088, "FDDetectorIsocenterPrimaryAngle" },
                                                            { 2505113624, "FDDetectorIsocenterSecondaryAngle" },
                                                            { 2505179160, "FDDetectorXPositionToIsocenter" },
                                                            { 2505244696, "FDDetectorYPositionToIsocenter" },
                                                            { 2505310232, "FDDetectorZPositionToIsocenter" },
                                                            { 2505375768, "SQXRayGridSequence" },
                                                            { 2505441304, "SQXRayFilterSequence" },
                                                            { 2505506840, "FDDetectorActiveAreaTLHCPosition" },
                                                            { 2505572376, "FDDetectorActiveAreaOrientation" },
                                                            { 2505637912, "CSPositionerPrimaryAngleDirection" },
                                                            { 2516647960, "SQDiffusionBMatrixSequence" },
                                                            { 2516713496, "FDDiffusionBValueXX" },
                                                            { 2516779032, "FDDiffusionBValueXY" },
                                                            { 2516844568, "FDDiffusionBValueXZ" },
                                                            { 2516910104, "FDDiffusionBValueYY" },
                                                            { 2516975640, "FDDiffusionBValueYZ" },
                                                            { 2517041176, "FDDiffusionBValueZZ" },
                                                            { 2518745112, "SQFunctionalMRSequence" },
                                                            { 2518810648, "CSFunctionalSettlingPhaseFramesPresent" },
                                                            { 2518876184, "DTFunctionalSyncPulse" },
                                                            { 2518941720, "CSSettlingPhaseFrame" },
                                                            { 2533425176, "DTDecayCorrectionDateTime" },
                                                            { 2534735896, "FDStartDensityThreshold" },
                                                            { 2534801432, "FDStartRelativeDensityDifferenceThreshold" },
                                                            { 2534866968, "FDStartCardiacTriggerCountThreshold" },
                                                            { 2534932504, "FDStartRespiratoryTriggerCountThreshold" },
                                                            { 2534998040, "FDTerminationCountsThreshold" },
                                                            { 2535456792, "FDTerminationDensityThreshold" },
                                                            { 2535522328, "FDTerminationRelativeDensityThreshold" },
                                                            { 2535587864, "FDTerminationTimeThreshold" },
                                                            { 2535653400, "FDTerminationCardiacTriggerCountThreshold" },
                                                            { 2535718936, "FDTerminationRespiratoryTriggerCountThreshold" },
                                                            { 2535784472, "CSDetectorGeometry" },
                                                            { 2535850008, "FDTransverseDetectorSeparation" },
                                                            { 2535915544, "FDAxialDetectorDimension" },
                                                            { 2536046616, "USRadiopharmaceuticalAgentNumber" },
                                                            { 2536636440, "SQPETFrameAcquisitionSequence" },
                                                            { 2536701976, "SQPETDetectorMotionDetailsSequence" },
                                                            { 2536767512, "SQPETTableDynamicsSequence" },
                                                            { 2536833048, "SQPETPositionSequence" },
                                                            { 2536898584, "SQPETFrameCorrectionFactorsSequence" },
                                                            { 2536964120, "SQRadiopharmaceuticalUsageSequence" },
                                                            { 2537029656, "CSAttenuationCorrectionSource" },
                                                            { 2537095192, "USNumberOfIterations" },
                                                            { 2537553944, "USNumberOfSubsets" },
                                                            { 2538143768, "SQPETReconstructionSequence" },
                                                            { 2538668056, "SQPETFrameTypeSequence" },
                                                            { 2538930200, "CSTimeOfFlightInformationUsed" },
                                                            { 2538995736, "CSReconstructionType" },
                                                            { 2539126808, "CSDecayCorrected" },
                                                            { 2539192344, "CSAttenuationCorrected" },
                                                            { 2539651096, "CSScatterCorrected" },
                                                            { 2539716632, "CSDeadTimeCorrected" },
                                                            { 2539782168, "CSGantryMotionCorrected" },
                                                            { 2539847704, "CSPatientMotionCorrected" },
                                                            { 2539913240, "CSCountLossNormalizationCorrected" },
                                                            { 2539978776, "CSRandomsCorrected" },
                                                            { 2540044312, "CSNonUniformRadialSamplingCorrected" },
                                                            { 2540109848, "CSSensitivityCalibrated" },
                                                            { 2540175384, "CSDetectorNormalizationCorrection" },
                                                            { 2540240920, "CSIterativeReconstructionMethod" },
                                                            { 2540699672, "CSAttenuationCorrectionTemporalRelationship" },
                                                            { 2540765208, "SQPatientPhysiologicalStateSequence" },
                                                            { 2540830744, "SQPatientPhysiologicalStateCodeSequence" },
                                                            { 2550202392, "FDDepthsOfFocus" },
                                                            { 2550333464, "SQExcludedIntervalsSequence" },
                                                            { 2550399000, "DTExclusionStartDateTime" },
                                                            { 2550464536, "FDExclusionDuration" },
                                                            { 2550530072, "SQUSImageDescriptionSequence" },
                                                            { 2550595608, "SQImageDataTypeSequence" },
                                                            { 2550661144, "CSDataType" },
                                                            { 2550726680, "SQTransducerScanPatternCodeSequence" },
                                                            { 2550857752, "CSAliasedDataType" },
                                                            { 2550923288, "CSPositionMeasuringDeviceUsed" },
                                                            { 2550988824, "SQTransducerGeometryCodeSequence" },
                                                            { 2551054360, "SQTransducerBeamSteeringCodeSequence" },
                                                            { 2551119896, "SQTransducerApplicationCodeSequence" },
                                                            { 2551185432, "USZeroVelocityPixelValue" },
                                                            { 2566914072, "LOReferenceLocationLabel" },
                                                            { 2566979608, "UTReferenceLocationDescription" },
                                                            { 2567045144, "SQReferenceBasisCodeSequence" },
                                                            { 2567110680, "SQReferenceGeometryCodeSequence" },
                                                            { 2567176216, "DSOffsetDistance" },
                                                            { 2567241752, "CSOffsetDirection" },
                                                            { 2567307288, "SQPotentialScheduledProtocolCodeSequence" },
                                                            { 2567372824, "SQPotentialRequestedProcedureCodeSequence" },
                                                            { 2567438360, "UCPotentialReasonsForProcedure" },
                                                            { 2567503896, "SQPotentialReasonsForProcedureCodeSequence" },
                                                            { 2567569432, "UCPotentialDiagnosticTasks" },
                                                            { 2567634968, "SQContraindicationsCodeSequence" },
                                                            { 2567700504, "SQReferencedDefinedProtocolSequence" },
                                                            { 2567766040, "SQReferencedPerformedProtocolSequence" },
                                                            { 2567831576, "SQPredecessorProtocolSequence" },
                                                            { 2567897112, "UTProtocolPlanningInformation" },
                                                            { 2567962648, "UTProtocolDesignRationale" },
                                                            { 2568028184, "SQPatientSpecificationSequence" },
                                                            { 2568093720, "SQModelSpecificationSequence" },
                                                            { 2568159256, "SQParametersSpecificationSequence" },
                                                            { 2568224792, "SQInstructionSequence" },
                                                            { 2568290328, "USInstructionIndex" },
                                                            { 2568355864, "LOInstructionText" },
                                                            { 2568421400, "UTInstructionDescription" },
                                                            { 2568486936, "CSInstructionPerformedFlag" },
                                                            { 2568552472, "DTInstructionPerformedDateTime" },
                                                            { 2568618008, "UTInstructionPerformanceComment" },
                                                            { 2568683544, "SQPatientPositioningInstructionSequence" },
                                                            { 2568749080, "SQPositioningMethodCodeSequence" },
                                                            { 2568814616, "SQPositioningLandmarkSequence" },
                                                            { 2568880152, "UITargetFrameOfReferenceUID" },
                                                            { 2568945688, "SQAcquisitionProtocolElementSpecificationSequence" },
                                                            { 2569011224, "SQAcquisitionProtocolElementSequence" },
                                                            { 2569076760, "USProtocolElementNumber" },
                                                            { 2569142296, "LOProtocolElementName" },
                                                            { 2569207832, "UTProtocolElementCharacteristicsSummary" },
                                                            { 2569273368, "UTProtocolElementPurpose" },
                                                            { 2570059800, "CSAcquisitionMotion" },
                                                            { 2570125336, "SQAcquisitionStartLocationSequence" },
                                                            { 2570190872, "SQAcquisitionEndLocationSequence" },
                                                            { 2570256408, "SQReconstructionProtocolElementSpecificationSequence" },
                                                            { 2570321944, "SQReconstructionProtocolElementSequence" },
                                                            { 2570387480, "SQStorageProtocolElementSpecificationSequence" },
                                                            { 2570453016, "SQStorageProtocolElementSequence" },
                                                            { 2570518552, "LORequestedSeriesDescription" },
                                                            { 2570584088, "USSourceAcquisitionProtocolElementNumber" },
                                                            { 2570649624, "USSourceAcquisitionBeamNumber" },
                                                            { 2570715160, "USSourceReconstructionProtocolElementNumber" },
                                                            { 2570780696, "SQReconstructionStartLocationSequence" },
                                                            { 2570846232, "SQReconstructionEndLocationSequence" },
                                                            { 2570911768, "SQReconstructionAlgorithmSequence" },
                                                            { 2570977304, "SQReconstructionTargetCenterLocationSequence" },
                                                            { 2571173912, "UTImageFilterDescription" },
                                                            { 2571239448, "FDCTDIvolNotificationTrigger" },
                                                            { 2571304984, "FDDLPNotificationTrigger" },
                                                            { 2571370520, "CSAutoKVPSelectionType" },
                                                            { 2571436056, "FDAutoKVPUpperBound" },
                                                            { 2571501592, "FDAutoKVPLowerBound" },
                                                            { 2571567128, "CSProtocolDefinedPatientPosition" },
                                                            { 2684420120, "SQContributingEquipmentSequence" },
                                                            { 2684485656, "DTContributionDateTime" },
                                                            { 2684551192, "STContributionDescription" },
                                                            { 852000, "UIStudyInstanceUID" },
                                                            { 917536, "UISeriesInstanceUID" },
                                                            { 1048608, "SHStudyID" },
                                                            { 1114144, "ISSeriesNumber" },
                                                            { 1179680, "ISAcquisitionNumber" },
                                                            { 1245216, "ISInstanceNumber" },
                                                            { 1310752, "ISIsotopeNumber" },
                                                            { 1376288, "ISPhaseNumber" },
                                                            { 1441824, "ISIntervalNumber" },
                                                            { 1507360, "ISTimeSlotNumber" },
                                                            { 1572896, "ISAngleNumber" },
                                                            { 1638432, "ISItemNumber" },
                                                            { 2097184, "CSPatientOrientation" },
                                                            { 2228256, "ISOverlayNumber" },
                                                            { 2359328, "ISCurveNumber" },
                                                            { 2490400, "ISLUTNumber" },
                                                            { 3145760, "DSImagePosition" },
                                                            { 3276832, "DSImagePositionPatient" },
                                                            { 3473440, "DSImageOrientation" },
                                                            { 3604512, "DSImageOrientationPatient" },
                                                            { 5242912, "DSLocation" },
                                                            { 5373984, "UIFrameOfReferenceUID" },
                                                            { 6291488, "CSLaterality" },
                                                            { 6422560, "CSImageLaterality" },
                                                            { 7340064, "LOImageGeometryType" },
                                                            { 8388640, "CSMaskingImage" },
                                                            { 11141152, "ISReportNumber" },
                                                            { 16777248, "ISTemporalPositionIdentifier" },
                                                            { 17104928, "ISNumberOfTemporalPositions" },
                                                            { 17825824, "DSTemporalResolution" },
                                                            { 33554464, "UISynchronizationFrameOfReferenceUID" },
                                                            { 37879840, "UISOPInstanceUIDOfConcatenationSource" },
                                                            { 268435488, "ISSeriesInStudy" },
                                                            { 268501024, "ISAcquisitionsInSeries" },
                                                            { 268566560, "ISImagesInAcquisition" },
                                                            { 268632096, "ISImagesInSeries" },
                                                            { 268697632, "ISAcquisitionsInStudy" },
                                                            { 268763168, "ISImagesInStudy" },
                                                            { 270532640, "LOReference" },
                                                            { 272564256, "LOTargetPositionReferenceIndicator" },
                                                            { 272629792, "LOPositionReferenceIndicator" },
                                                            { 272695328, "DSSliceLocation" },
                                                            { 275775520, "ISOtherStudyNumbers" },
                                                            { 301989920, "ISNumberOfPatientRelatedStudies" },
                                                            { 302120992, "ISNumberOfPatientRelatedSeries" },
                                                            { 302252064, "ISNumberOfPatientRelatedInstances" },
                                                            { 302383136, "ISNumberOfStudyRelatedSeries" },
                                                            { 302514208, "ISNumberOfStudyRelatedInstances" },
                                                            { 302579744, "ISNumberOfSeriesRelatedInstances" },
                                                            { 822083616, "CSSourceImageIDs" },
                                                            { 872480800, "CSModifyingDeviceID" },
                                                            { 872546336, "CSModifiedImageID" },
                                                            { 872611872, "DAModifiedImageDate" },
                                                            { 872677408, "LOModifyingDeviceManufacturer" },
                                                            { 872742944, "TMModifiedImageTime" },
                                                            { 872808480, "LOModifiedImageDescription" },
                                                            { 1073741856, "LTImageComments" },
                                                            { 1342177312, "ATOriginalImageIdentification" },
                                                            { 1342308384, "LOOriginalImageIdentificationNomenclature" },
                                                            { 2421555232, "SHStackID" },
                                                            { 2421620768, "ULInStackPositionNumber" },
                                                            { 2423324704, "SQFrameAnatomySequence" },
                                                            { 2423390240, "CSFrameLaterality" },
                                                            { 2433810464, "SQFrameContentSequence" },
                                                            { 2433941536, "SQPlanePositionSequence" },
                                                            { 2434138144, "SQPlaneOrientationSequence" },
                                                            { 2435317792, "ULTemporalPositionIndex" },
                                                            { 2438135840, "FDNominalCardiacTriggerDelayTime" },
                                                            { 2438201376, "FLNominalCardiacTriggerTimePriorToRPeak" },
                                                            { 2438266912, "FLActualCardiacTriggerTimePriorToRPeak" },
                                                            { 2438332448, "USFrameAcquisitionNumber" },
                                                            { 2438397984, "ULDimensionIndexValues" },
                                                            { 2438463520, "LTFrameComments" },
                                                            { 2439053344, "UIConcatenationUID" },
                                                            { 2439118880, "USInConcatenationNumber" },
                                                            { 2439184416, "USInConcatenationTotalNumber" },
                                                            { 2439249952, "UIDimensionOrganizationUID" },
                                                            { 2439315488, "ATDimensionIndexPointer" },
                                                            { 2439446560, "ATFunctionalGroupPointer" },
                                                            { 2440036384, "SQUnassignedSharedConvertedAttributesSequence" },
                                                            { 2440101920, "SQUnassignedPerFrameConvertedAttributesSequence" },
                                                            { 2440167456, "SQConversionSourceAttributesSequence" },
                                                            { 2450718752, "LODimensionIndexPrivateCreator" },
                                                            { 2451636256, "SQDimensionOrganizationSequence" },
                                                            { 2451701792, "SQDimensionIndexSequence" },
                                                            { 2452095008, "ULConcatenationFrameOffsetNumber" },
                                                            { 2453143584, "LOFunctionalGroupPrivateCreator" },
                                                            { 2453733408, "FLNominalPercentageOfCardiacPhase" },
                                                            { 2453995552, "FLNominalPercentageOfRespiratoryPhase" },
                                                            { 2454061088, "FLStartingRespiratoryAmplitude" },
                                                            { 2454126624, "CSStartingRespiratoryPhase" },
                                                            { 2454192160, "FLEndingRespiratoryAmplitude" },
                                                            { 2454257696, "CSEndingRespiratoryPhase" },
                                                            { 2454716448, "CSRespiratoryTriggerType" },
                                                            { 2454781984, "FDRRIntervalTimeNominal" },
                                                            { 2454847520, "FDActualCardiacTriggerDelayTime" },
                                                            { 2454913056, "SQRespiratorySynchronizationSequence" },
                                                            { 2454978592, "FDRespiratoryIntervalTime" },
                                                            { 2455044128, "FDNominalRespiratoryTriggerDelayTime" },
                                                            { 2455109664, "FDRespiratoryTriggerDelayThreshold" },
                                                            { 2455175200, "FDActualRespiratoryTriggerDelayTime" },
                                                            { 2466316320, "FDImagePositionVolume" },
                                                            { 2466381856, "FDImageOrientationVolume" },
                                                            { 2466709536, "CSUltrasoundAcquisitionGeometry" },
                                                            { 2466775072, "FDApexPosition" },
                                                            { 2466840608, "FDVolumeToTransducerMappingMatrix" },
                                                            { 2466906144, "FDVolumeToTableMappingMatrix" },
                                                            { 2466971680, "CSVolumeToTransducerRelationship" },
                                                            { 2467037216, "CSPatientFrameOfReferenceSource" },
                                                            { 2467102752, "FDTemporalPositionTimeOffset" },
                                                            { 2467168288, "SQPlanePositionVolumeSequence" },
                                                            { 2467233824, "SQPlaneOrientationVolumeSequence" },
                                                            { 2467299360, "SQTemporalPositionSequence" },
                                                            { 2467364896, "CSDimensionOrganizationType" },
                                                            { 2467430432, "UIVolumeFrameOfReferenceUID" },
                                                            { 2467495968, "UITableFrameOfReferenceUID" },
                                                            { 2485190688, "LODimensionDescriptionLabel" },
                                                            { 2488270880, "SQPatientOrientationInFrameSequence" },
                                                            { 2488467488, "LOFrameLabel" },
                                                            { 2501378080, "USAcquisitionIndex" },
                                                            { 2502492192, "SQContributingSOPInstancesReferenceSequence" },
                                                            { 2503344160, "USReconstructionIndex" },
                                                            { 65570, "USLightPathFilterPassThroughWavelength" },
                                                            { 131106, "USLightPathFilterPassBand" },
                                                            { 196642, "USImagePathFilterPassThroughWavelength" },
                                                            { 262178, "USImagePathFilterPassBand" },
                                                            { 327714, "CSPatientEyeMovementCommanded" },
                                                            { 393250, "SQPatientEyeMovementCommandCodeSequence" },
                                                            { 458786, "FLSphericalLensPower" },
                                                            { 524322, "FLCylinderLensPower" },
                                                            { 589858, "FLCylinderAxis" },
                                                            { 655394, "FLEmmetropicMagnification" },
                                                            { 720930, "FLIntraOcularPressure" },
                                                            { 786466, "FLHorizontalFieldOfView" },
                                                            { 852002, "CSPupilDilated" },
                                                            { 917538, "FLDegreeOfDilation" },
                                                            { 1048610, "FLStereoBaselineAngle" },
                                                            { 1114146, "FLStereoBaselineDisplacement" },
                                                            { 1179682, "FLStereoHorizontalPixelOffset" },
                                                            { 1245218, "FLStereoVerticalPixelOffset" },
                                                            { 1310754, "FLStereoRotation" },
                                                            { 1376290, "SQAcquisitionDeviceTypeCodeSequence" },
                                                            { 1441826, "SQIlluminationTypeCodeSequence" },
                                                            { 1507362, "SQLightPathFilterTypeStackCodeSequence" },
                                                            { 1572898, "SQImagePathFilterTypeStackCodeSequence" },
                                                            { 1638434, "SQLensesCodeSequence" },
                                                            { 1703970, "SQChannelDescriptionCodeSequence" },
                                                            { 1769506, "SQRefractiveStateSequence" },
                                                            { 1835042, "SQMydriaticAgentCodeSequence" },
                                                            { 1900578, "SQRelativeImagePositionCodeSequence" },
                                                            { 1966114, "FLCameraAngleOfView" },
                                                            { 2097186, "SQStereoPairsSequence" },
                                                            { 2162722, "SQLeftImageSequence" },
                                                            { 2228258, "SQRightImageSequence" },
                                                            { 2621474, "CSStereoPairsPresent" },
                                                            { 3145762, "FLAxialLengthOfTheEye" },
                                                            { 3211298, "SQOphthalmicFrameLocationSequence" },
                                                            { 3276834, "FLReferenceCoordinates" },
                                                            { 3473442, "FLDepthSpatialResolution" },
                                                            { 3538978, "FLMaximumDepthDistortion" },
                                                            { 3604514, "FLAlongScanSpatialResolution" },
                                                            { 3670050, "FLMaximumAlongScanDistortion" },
                                                            { 3735586, "CSOphthalmicImageOrientation" },
                                                            { 4259874, "FLDepthOfTransverseImage" },
                                                            { 4325410, "SQMydriaticAgentConcentrationUnitsSequence" },
                                                            { 4718626, "FLAcrossScanSpatialResolution" },
                                                            { 4784162, "FLMaximumAcrossScanDistortion" },
                                                            { 5111842, "DSMydriaticAgentConcentration" },
                                                            { 5570594, "FLIlluminationWaveLength" },
                                                            { 5636130, "FLIlluminationPower" },
                                                            { 5701666, "FLIlluminationBandwidth" },
                                                            { 5767202, "SQMydriaticAgentSequence" },
                                                            { 268894242, "SQOphthalmicAxialMeasurementsRightEyeSequence" },
                                                            { 268959778, "SQOphthalmicAxialMeasurementsLeftEyeSequence" },
                                                            { 269025314, "CSOphthalmicAxialMeasurementsDeviceType" },
                                                            { 269484066, "CSOphthalmicAxialLengthMeasurementsType" },
                                                            { 269615138, "SQOphthalmicAxialLengthSequence" },
                                                            { 270073890, "FLOphthalmicAxialLength" },
                                                            { 270794786, "SQLensStatusCodeSequence" },
                                                            { 270860322, "SQVitreousStatusCodeSequence" },
                                                            { 271056930, "SQIOLFormulaCodeSequence" },
                                                            { 271122466, "LOIOLFormulaDetail" },
                                                            { 271777826, "FLKeratometerIndex" },
                                                            { 271908898, "SQSourceOfOphthalmicAxialLengthCodeSequence" },
                                                            { 272039970, "FLTargetRefraction" },
                                                            { 272171042, "CSRefractiveProcedureOccurred" },
                                                            { 272629794, "SQRefractiveSurgeryTypeCodeSequence" },
                                                            { 272891938, "SQOphthalmicUltrasoundMethodCodeSequence" },
                                                            { 273678370, "SQOphthalmicAxialLengthMeasurementsSequence" },
                                                            { 273874978, "FLIOLPower" },
                                                            { 273940514, "FLPredictedRefractiveError" },
                                                            { 274268194, "FLOphthalmicAxialLengthVelocity" },
                                                            { 275054626, "LOLensStatusDescription" },
                                                            { 275120162, "LOVitreousStatusDescription" },
                                                            { 277872674, "SQIOLPowerSequence" },
                                                            { 278003746, "SQLensConstantSequence" },
                                                            { 278069282, "LOIOLManufacturer" },
                                                            { 278134818, "LOLensConstantDescription" },
                                                            { 278200354, "LOImplantName" },
                                                            { 278265890, "SQKeratometryMeasurementTypeCodeSequence" },
                                                            { 278331426, "LOImplantPartNumber" },
                                                            { 285212706, "SQReferencedOphthalmicAxialMeasurementsSequence" },
                                                            { 285278242, "SQOphthalmicAxialLengthMeasurementsSegmentNameCodeSequence" },
                                                            { 285409314, "SQRefractiveErrorBeforeRefractiveSurgeryCodeSequence" },
                                                            { 287375394, "FLIOLPowerForExactEmmetropia" },
                                                            { 287440930, "FLIOLPowerForExactTargetRefraction" },
                                                            { 287637538, "SQAnteriorChamberDepthDefinitionCodeSequence" },
                                                            { 287768610, "SQLensThicknessSequence" },
                                                            { 287834146, "SQAnteriorChamberDepthSequence" },
                                                            { 288358434, "FLLensThickness" },
                                                            { 288423970, "FLAnteriorChamberDepth" },
                                                            { 288489506, "SQSourceOfLensThicknessDataCodeSequence" },
                                                            { 288555042, "SQSourceOfAnteriorChamberDepthDataCodeSequence" },
                                                            { 288620578, "SQSourceOfRefractiveMeasurementsSequence" },
                                                            { 288686114, "SQSourceOfRefractiveMeasurementsCodeSequence" },
                                                            { 289407010, "CSOphthalmicAxialLengthMeasurementModified" },
                                                            { 290455586, "SQOphthalmicAxialLengthDataSourceCodeSequence" },
                                                            { 290652194, "SQOphthalmicAxialLengthAcquisitionMethodCodeSequence" },
                                                            { 290783266, "FLSignalToNoiseRatio" },
                                                            { 291045410, "LOOphthalmicAxialLengthDataSourceDescription" },
                                                            { 303038498, "SQOphthalmicAxialLengthMeasurementsTotalLengthSequence" },
                                                            { 303104034, "SQOphthalmicAxialLengthMeasurementsSegmentalLengthSequence" },
                                                            { 303169570, "SQOphthalmicAxialLengthMeasurementsLengthSummationSequence" },
                                                            { 304087074, "SQUltrasoundOphthalmicAxialLengthMeasurementsSequence" },
                                                            { 304414754, "SQOpticalOphthalmicAxialLengthMeasurementsSequence" },
                                                            { 305135650, "SQUltrasoundSelectedOphthalmicAxialLengthSequence" },
                                                            { 307232802, "SQOphthalmicAxialLengthSelectionMethodCodeSequence" },
                                                            { 307560482, "SQOpticalSelectedOphthalmicAxialLengthSequence" },
                                                            { 307691554, "SQSelectedSegmentalOphthalmicAxialLengthSequence" },
                                                            { 308281378, "SQSelectedTotalOphthalmicAxialLengthSequence" },
                                                            { 308412450, "SQOphthalmicAxialLengthQualityMetricSequence" },
                                                            { 308609058, "SQOphthalmicAxialLengthQualityMetricTypeCodeSequence" },
                                                            { 309526562, "LOOphthalmicAxialLengthQualityMetricTypeDescription" },
                                                            { 318767138, "SQIntraocularLensCalculationsRightEyeSequence" },
                                                            { 319815714, "SQIntraocularLensCalculationsLeftEyeSequence" },
                                                            { 321912866, "SQReferencedOphthalmicAxialLengthMeasurementQCImageSequence" },
                                                            { 336920610, "CSOphthalmicMappingDeviceType" },
                                                            { 337641506, "SQAcquisitionMethodCodeSequence" },
                                                            { 337838114, "SQAcquisitionMethodAlgorithmSequence" },
                                                            { 339083298, "SQOphthalmicThicknessMapTypeCodeSequence" },
                                                            { 339935266, "SQOphthalmicThicknessMappingNormalsSequence" },
                                                            { 340066338, "SQRetinalThicknessDefinitionCodeSequence" },
                                                            { 340787234, "SQPixelValueMappingToCodedConceptSequence" },
                                                            { 340918306, "USMappedPixelValue" },
                                                            { 341049378, "LOPixelValueMappingExplanation" },
                                                            { 341311522, "SQOphthalmicThicknessMapQualityThresholdSequence" },
                                                            { 341835810, "FLOphthalmicThicknessMapThresholdQualityRating" },
                                                            { 342032418, "FLAnatomicStructureReferencePoint" },
                                                            { 342163490, "SQRegistrationToLocalizerSequence" },
                                                            { 342229026, "CSRegisteredLocalizerUnits" },
                                                            { 342294562, "FLRegisteredLocalizerTopLeftHandCorner" },
                                                            { 342360098, "FLRegisteredLocalizerBottomRightHandCorner" },
                                                            { 342884386, "SQOphthalmicThicknessMapQualityRatingSequence" },
                                                            { 343015458, "SQRelevantOPTAttributesSequence" },
                                                            { 353501218, "SQTransformationMethodCodeSequence" },
                                                            { 353566754, "SQTransformationAlgorithmSequence" },
                                                            { 353697826, "CSOphthalmicAxialLengthMethod" },
                                                            { 353828898, "FLOphthalmicFOV" },
                                                            { 353894434, "SQTwoDimensionalToThreeDimensionalMapSequence" },
                                                            { 354746402, "SQWideFieldOphthalmicPhotographyQualityRatingSequence" },
                                                            { 354811938, "SQWideFieldOphthalmicPhotographyQualityThresholdSequence" },
                                                            { 354877474, "FLWideFieldOphthalmicPhotographyThresholdQualityRating" },
                                                            { 354943010, "FLXCoordinatesCenterPixelViewAngle" },
                                                            { 355008546, "FLYCoordinatesCenterPixelViewAngle" },
                                                            { 355467298, "ULNumberOfMapPoints" },
                                                            { 355532834, "OFTwoDimensionalToThreeDimensionalMapData" },
                                                            { 1048612, "FLVisualFieldHorizontalExtent" },
                                                            { 1114148, "FLVisualFieldVerticalExtent" },
                                                            { 1179684, "CSVisualFieldShape" },
                                                            { 1441828, "SQScreeningTestModeCodeSequence" },
                                                            { 1572900, "FLMaximumStimulusLuminance" },
                                                            { 2097188, "FLBackgroundLuminance" },
                                                            { 2162724, "SQStimulusColorCodeSequence" },
                                                            { 2359332, "SQBackgroundIlluminationColorCodeSequence" },
                                                            { 2424868, "FLStimulusArea" },
                                                            { 2621476, "FLStimulusPresentationTime" },
                                                            { 3276836, "SQFixationSequence" },
                                                            { 3342372, "SQFixationMonitoringCodeSequence" },
                                                            { 3407908, "SQVisualFieldCatchTrialSequence" },
                                                            { 3473444, "USFixationCheckedQuantity" },
                                                            { 3538980, "USPatientNotProperlyFixatedQuantity" },
                                                            { 3604516, "CSPresentedVisualStimuliDataFlag" },
                                                            { 3670052, "USNumberOfVisualStimuli" },
                                                            { 3735588, "CSExcessiveFixationLossesDataFlag" },
                                                            { 4194340, "CSExcessiveFixationLosses" },
                                                            { 4325412, "USStimuliRetestingQuantity" },
                                                            { 4456484, "LTCommentsOnPatientPerformanceOfVisualField" },
                                                            { 4522020, "CSFalseNegativesEstimateFlag" },
                                                            { 4587556, "FLFalseNegativesEstimate" },
                                                            { 4718628, "USNegativeCatchTrialsQuantity" },
                                                            { 5242916, "USFalseNegativesQuantity" },
                                                            { 5308452, "CSExcessiveFalseNegativesDataFlag" },
                                                            { 5373988, "CSExcessiveFalseNegatives" },
                                                            { 5439524, "CSFalsePositivesEstimateFlag" },
                                                            { 5505060, "FLFalsePositivesEstimate" },
                                                            { 5570596, "CSCatchTrialsDataFlag" },
                                                            { 5636132, "USPositiveCatchTrialsQuantity" },
                                                            { 5701668, "CSTestPointNormalsDataFlag" },
                                                            { 5767204, "SQTestPointNormalsSequence" },
                                                            { 5832740, "CSGlobalDeviationProbabilityNormalsFlag" },
                                                            { 6291492, "USFalsePositivesQuantity" },
                                                            { 6357028, "CSExcessiveFalsePositivesDataFlag" },
                                                            { 6422564, "CSExcessiveFalsePositives" },
                                                            { 6488100, "CSVisualFieldTestNormalsFlag" },
                                                            { 6553636, "SQResultsNormalsSequence" },
                                                            { 6619172, "SQAgeCorrectedSensitivityDeviationAlgorithmSequence" },
                                                            { 6684708, "FLGlobalDeviationFromNormal" },
                                                            { 6750244, "SQGeneralizedDefectSensitivityDeviationAlgorithmSequence" },
                                                            { 6815780, "FLLocalizedDeviationFromNormal" },
                                                            { 6881316, "LOPatientReliabilityIndicator" },
                                                            { 7340068, "FLVisualFieldMeanSensitivity" },
                                                            { 7405604, "FLGlobalDeviationProbability" },
                                                            { 7471140, "CSLocalDeviationProbabilityNormalsFlag" },
                                                            { 7536676, "FLLocalizedDeviationProbability" },
                                                            { 7602212, "CSShortTermFluctuationCalculated" },
                                                            { 7667748, "FLShortTermFluctuation" },
                                                            { 7733284, "CSShortTermFluctuationProbabilityCalculated" },
                                                            { 7798820, "FLShortTermFluctuationProbability" },
                                                            { 7864356, "CSCorrectedLocalizedDeviationFromNormalCalculated" },
                                                            { 7929892, "FLCorrectedLocalizedDeviationFromNormal" },
                                                            { 8388644, "CSCorrectedLocalizedDeviationFromNormalProbabilityCalculated" },
                                                            { 8454180, "FLCorrectedLocalizedDeviationFromNormalProbability" },
                                                            { 8585252, "SQGlobalDeviationProbabilitySequence" },
                                                            { 8716324, "SQLocalizedDeviationProbabilitySequence" },
                                                            { 8781860, "CSFovealSensitivityMeasured" },
                                                            { 8847396, "FLFovealSensitivity" },
                                                            { 8912932, "FLVisualFieldTestDuration" },
                                                            { 8978468, "SQVisualFieldTestPointSequence" },
                                                            { 9437220, "FLVisualFieldTestPointXCoordinate" },
                                                            { 9502756, "FLVisualFieldTestPointYCoordinate" },
                                                            { 9568292, "FLAgeCorrectedSensitivityDeviationValue" },
                                                            { 9633828, "CSStimulusResults" },
                                                            { 9699364, "FLSensitivityValue" },
                                                            { 9764900, "CSRetestStimulusSeen" },
                                                            { 9830436, "FLRetestSensitivityValue" },
                                                            { 9895972, "SQVisualFieldTestPointNormalsSequence" },
                                                            { 9961508, "FLQuantifiedDefect" },
                                                            { 16777252, "FLAgeCorrectedSensitivityDeviationProbabilityValue" },
                                                            { 16908324, "CSGeneralizedDefectCorrectedSensitivityDeviationFlag" },
                                                            { 16973860, "FLGeneralizedDefectCorrectedSensitivityDeviationValue" },
                                                            { 17039396, "FLGeneralizedDefectCorrectedSensitivityDeviationProbabilityValue" },
                                                            { 17104932, "FLMinimumSensitivityValue" },
                                                            { 17170468, "CSBlindSpotLocalized" },
                                                            { 17236004, "FLBlindSpotXCoordinate" },
                                                            { 17301540, "FLBlindSpotYCoordinate" },
                                                            { 17825828, "SQVisualAcuityMeasurementSequence" },
                                                            { 17956900, "SQRefractiveParametersUsedOnPatientSequence" },
                                                            { 18022436, "CSMeasurementLaterality" },
                                                            { 18087972, "SQOphthalmicPatientClinicalInformationLeftEyeSequence" },
                                                            { 18153508, "SQOphthalmicPatientClinicalInformationRightEyeSequence" },
                                                            { 18284580, "CSFovealPointNormativeDataFlag" },
                                                            { 18350116, "FLFovealPointProbabilityValue" },
                                                            { 18874404, "CSScreeningBaselineMeasured" },
                                                            { 19005476, "SQScreeningBaselineMeasuredSequence" },
                                                            { 19136548, "CSScreeningBaselineType" },
                                                            { 19267620, "FLScreeningBaselineValue" },
                                                            { 33685540, "LOAlgorithmSource" },
                                                            { 50724900, "LODataSetName" },
                                                            { 50790436, "LODataSetVersion" },
                                                            { 50855972, "LODataSetSource" },
                                                            { 50921508, "LODataSetDescription" },
                                                            { 51839012, "SQVisualFieldTestReliabilityGlobalIndexSequence" },
                                                            { 52428836, "SQVisualFieldGlobalResultsIndexSequence" },
                                                            { 52756516, "SQDataObservationSequence" },
                                                            { 54001700, "CSIndexNormalsFlag" },
                                                            { 54591524, "FLIndexProbability" },
                                                            { 54788132, "SQIndexProbabilitySequence" },
                                                            { 131112, "USSamplesPerPixel" },
                                                            { 196648, "USSamplesPerPixelUsed" },
                                                            { 262184, "CSPhotometricInterpretation" },
                                                            { 327720, "USImageDimensions" },
                                                            { 393256, "USPlanarConfiguration" },
                                                            { 524328, "ISNumberOfFrames" },
                                                            { 589864, "ATFrameIncrementPointer" },
                                                            { 655400, "ATFrameDimensionPointer" },
                                                            { 1048616, "USRows" },
                                                            { 1114152, "USColumns" },
                                                            { 1179688, "USPlanes" },
                                                            { 1310760, "USUltrasoundColorDataPresent" },
                                                            { 3145768, "DSPixelSpacing" },
                                                            { 3211304, "DSZoomFactor" },
                                                            { 3276840, "DSZoomCenter" },
                                                            { 3407912, "ISPixelAspectRatio" },
                                                            { 4194344, "CSImageFormat" },
                                                            { 5242920, "LOManipulatedImage" },
                                                            { 5308456, "CSCorrectedImage" },
                                                            { 6225960, "LOCompressionRecognitionCode" },
                                                            { 6291496, "CSCompressionCode" },
                                                            { 6357032, "SHCompressionOriginator" },
                                                            { 6422568, "LOCompressionLabel" },
                                                            { 6488104, "SHCompressionDescription" },
                                                            { 6619176, "CSCompressionSequence" },
                                                            { 6684712, "ATCompressionStepPointers" },
                                                            { 6815784, "USRepeatInterval" },
                                                            { 6881320, "USBitsGrouped" },
                                                            { 7340072, "USPerimeterTable" },
                                                            { 7405608, "USPerimeterValue" },
                                                            { 8388648, "USPredictorRows" },
                                                            { 8454184, "USPredictorColumns" },
                                                            { 8519720, "USPredictorConstants" },
                                                            { 9437224, "CSBlockedPixels" },
                                                            { 9502760, "USBlockRows" },
                                                            { 9568296, "USBlockColumns" },
                                                            { 9633832, "USRowOverlap" },
                                                            { 9699368, "USColumnOverlap" },
                                                            { 16777256, "USBitsAllocated" },
                                                            { 16842792, "USBitsStored" },
                                                            { 16908328, "USHighBit" },
                                                            { 16973864, "USPixelRepresentation" },
                                                            { 17039400, "USSmallestValidPixelValue" },
                                                            { 17104936, "USLargestValidPixelValue" },
                                                            { 17170472, "USSmallestImagePixelValue" },
                                                            { 17236008, "USLargestImagePixelValue" },
                                                            { 17301544, "USSmallestPixelValueInSeries" },
                                                            { 17367080, "USLargestPixelValueInSeries" },
                                                            { 17825832, "USSmallestImagePixelValueInPlane" },
                                                            { 17891368, "USLargestImagePixelValueInPlane" },
                                                            { 18874408, "USPixelPaddingValue" },
                                                            { 18939944, "USPixelPaddingRangeLimit" },
                                                            { 19005480, "FLFloatPixelPaddingValue" },
                                                            { 19071016, "FDDoubleFloatPixelPaddingValue" },
                                                            { 19136552, "FLFloatPixelPaddingRangeLimit" },
                                                            { 19202088, "FDDoubleFloatPixelPaddingRangeLimit" },
                                                            { 33554472, "USImageLocation" },
                                                            { 50331688, "CSQualityControlImage" },
                                                            { 50397224, "CSBurnedInAnnotation" },
                                                            { 50462760, "CSRecognizableVisualFeatures" },
                                                            { 50528296, "CSLongitudinalTemporalInformationModified" },
                                                            { 50593832, "UIReferencedColorPaletteInstanceUID" },
                                                            { 67108904, "LOTransformLabel" },
                                                            { 67174440, "LOTransformVersionNumber" },
                                                            { 67239976, "USNumberOfTransformSteps" },
                                                            { 67305512, "LOSequenceOfCompressedData" },
                                                            { 67371048, "ATDetailsOfCoefficients" },
                                                            //{ 67108904, "USRowsForNthOrderCoefficients" },
                                                            //{ 67174440, "USColumnsForNthOrderCoefficients" },
                                                            //{ 67239976, "LOCoefficientCoding" },
                                                            //{ 67305512, "ATCoefficientCodingPointers" },
                                                            { 117440552, "LODCTLabel" },
                                                            { 117506088, "CSDataBlockDescription" },
                                                            { 117571624, "ATDataBlock" },
                                                            { 118489128, "USNormalizationFactorFormat" },
                                                            { 119537704, "USZonalMapNumberFormat" },
                                                            { 119603240, "ATZonalMapLocation" },
                                                            { 119668776, "USZonalMapFormat" },
                                                            { 120586280, "USAdaptiveMapFormat" },
                                                            { 121634856, "USCodeNumberFormat" },
                                                            { 134217768, "CSCodeLabel" },
                                                            { 134348840, "USNumberOfTables" },
                                                            { 134414376, "ATCodeTableLocation" },
                                                            { 134479912, "USBitsForCodeWord" },
                                                            { 134742056, "ATImageDataLocation" },
                                                            { 167903272, "CSPixelSpacingCalibrationType" },
                                                            { 168034344, "LOPixelSpacingCalibrationDescription" },
                                                            { 272629800, "CSPixelIntensityRelationship" },
                                                            { 272695336, "SSPixelIntensityRelationshipSign" },
                                                            { 273678376, "DSWindowCenter" },
                                                            { 273743912, "DSWindowWidth" },
                                                            { 273809448, "DSRescaleIntercept" },
                                                            { 273874984, "DSRescaleSlope" },
                                                            { 273940520, "LORescaleType" },
                                                            { 274006056, "LOWindowCenterWidthExplanation" },
                                                            { 274071592, "CSVOILUTFunction" },
                                                            { 276824104, "CSGrayScale" },
                                                            { 277872680, "CSRecommendedViewingMode" },
                                                            { 285212712, "USGrayLookupTableDescriptor" },
                                                            { 285278248, "USRedPaletteColorLookupTableDescriptor" },
                                                            { 285343784, "USGreenPaletteColorLookupTableDescriptor" },
                                                            { 285409320, "USBluePaletteColorLookupTableDescriptor" },
                                                            { 285474856, "USAlphaPaletteColorLookupTableDescriptor" },
                                                            { 286326824, "USLargeRedPaletteColorLookupTableDescriptor" },
                                                            { 286392360, "USLargeGreenPaletteColorLookupTableDescriptor" },
                                                            { 286457896, "USLargeBluePaletteColorLookupTableDescriptor" },
                                                            { 295239720, "UIPaletteColorLookupTableUID" },
                                                            { 301989928, "USGrayLookupTableData" },
                                                            { 302055464, "OWRedPaletteColorLookupTableData" },
                                                            { 302121000, "OWGreenPaletteColorLookupTableData" },
                                                            { 302186536, "OWBluePaletteColorLookupTableData" },
                                                            { 302252072, "OWAlphaPaletteColorLookupTableData" },
                                                            { 303104040, "OWLargeRedPaletteColorLookupTableData" },
                                                            { 303169576, "OWLargeGreenPaletteColorLookupTableData" },
                                                            { 303235112, "OWLargeBluePaletteColorLookupTableData" },
                                                            { 303300648, "UILargePaletteColorLookupTableUID" },
                                                            { 304152616, "OWSegmentedRedPaletteColorLookupTableData" },
                                                            { 304218152, "OWSegmentedGreenPaletteColorLookupTableData" },
                                                            { 304283688, "OWSegmentedBluePaletteColorLookupTableData" },
                                                            { 304349224, "OWSegmentedAlphaPaletteColorLookupTableData" },
                                                            { 305135656, "SQStoredValueColorRangeSequence" },
                                                            { 305201192, "FDMinimumStoredValueMapped" },
                                                            { 305266728, "FDMaximumStoredValueMapped" },
                                                            { 318767144, "CSBreastImplantPresent" },
                                                            { 324010024, "CSPartialView" },
                                                            { 324075560, "STPartialViewDescription" },
                                                            { 324141096, "SQPartialViewCodeSequence" },
                                                            { 324665384, "CSSpatialLocationsPreserved" },
                                                            { 335609896, "SQDataFrameAssignmentSequence" },
                                                            { 335675432, "CSDataPathAssignment" },
                                                            { 335740968, "USBitsMappedToColorLookupTable" },
                                                            { 335806504, "SQBlendingLUT1Sequence" },
                                                            { 335872040, "CSBlendingLUT1TransferFunction" },
                                                            { 335937576, "FDBlendingWeightConstant" },
                                                            { 336003112, "USBlendingLookupTableDescriptor" },
                                                            { 336068648, "OWBlendingLookupTableData" },
                                                            { 336265256, "SQEnhancedPaletteColorLookupTableSequence" },
                                                            { 336330792, "SQBlendingLUT2Sequence" },
                                                            { 336396328, "CSBlendingLUT2TransferFunction" },
                                                            { 336461864, "CSDataPathID" },
                                                            { 336527400, "CSRGBLUTTransferFunction" },
                                                            { 336592936, "CSAlphaLUTTransferFunction" },
                                                            { 536870952, "OBICCProfile" },
                                                            { 537002024, "CSColorSpace" },
                                                            { 554696744, "CSLossyImageCompression" },
                                                            { 554827816, "DSLossyImageCompressionRatio" },
                                                            { 554958888, "CSLossyImageCompressionMethod" },
                                                            { 805306408, "SQModalityLUTSequence" },
                                                            { 805437480, "USLUTDescriptor" },
                                                            { 805503016, "LOLUTExplanation" },
                                                            { 805568552, "LOModalityLUTType" },
                                                            { 805699624, "USLUTData" },
                                                            { 806354984, "SQVOILUTSequence" },
                                                            { 823132200, "SQSoftcopyVOILUTSequence" },
                                                            { 1073741864, "LTImagePresentationComments" },
                                                            { 1342177320, "SQBiPlaneAcquisitionSequence" },
                                                            { 1611661352, "USRepresentativeFrameNumber" },
                                                            { 1612709928, "USFrameNumbersOfInterest" },
                                                            { 1612841000, "LOFrameOfInterestDescription" },
                                                            { 1612906536, "CSFrameOfInterestType" },
                                                            { 1613758504, "USMaskPointers" },
                                                            { 1614807080, "USRWavePointer" },
                                                            { 1627389992, "SQMaskSubtractionSequence" },
                                                            { 1627455528, "CSMaskOperation" },
                                                            { 1627521064, "USApplicableFrameRange" },
                                                            { 1628438568, "USMaskFrameNumbers" },
                                                            { 1628569640, "USContrastFrameAveraging" },
                                                            { 1628700712, "FLMaskSubPixelShift" },
                                                            { 1629487144, "SSTIDOffset" },
                                                            { 1636827176, "STMaskOperationExplanation" },
                                                            { 1879048232, "SQEquipmentAdministratorSequence" },
                                                            { 1879113768, "USNumberOfDisplaySubsystems" },
                                                            { 1879179304, "USCurrentConfigurationID" },
                                                            { 1879244840, "USDisplaySubsystemID" },
                                                            { 1879310376, "SHDisplaySubsystemName" },
                                                            { 1879375912, "LODisplaySubsystemDescription" },
                                                            { 1879441448, "CSSystemStatus" },
                                                            { 1879506984, "LOSystemStatusComment" },
                                                            { 1879572520, "SQTargetLuminanceCharacteristicsSequence" },
                                                            { 1879638056, "USLuminanceCharacteristicsID" },
                                                            { 1879703592, "SQDisplaySubsystemConfigurationSequence" },
                                                            { 1879769128, "USConfigurationID" },
                                                            { 1879834664, "SHConfigurationName" },
                                                            { 1879900200, "LOConfigurationDescription" },
                                                            { 1879965736, "USReferencedTargetLuminanceCharacteristicsID" },
                                                            { 1880031272, "SQQAResultsSequence" },
                                                            { 1880096808, "SQDisplaySubsystemQAResultsSequence" },
                                                            { 1880162344, "SQConfigurationQAResultsSequence" },
                                                            { 1880227880, "SQMeasurementEquipmentSequence" },
                                                            { 1880293416, "CSMeasurementFunctions" },
                                                            { 1880358952, "CSMeasurementEquipmentType" },
                                                            { 1880424488, "SQVisualEvaluationResultSequence" },
                                                            { 1880490024, "SQDisplayCalibrationResultSequence" },
                                                            { 1880555560, "USDDLValue" },
                                                            { 1880621096, "FLCIExyWhitePoint" },
                                                            { 1880686632, "CSDisplayFunctionType" },
                                                            { 1880752168, "FLGammaValue" },
                                                            { 1880817704, "USNumberOfLuminancePoints" },
                                                            { 1880883240, "SQLuminanceResponseSequence" },
                                                            { 1880948776, "FLTargetMinimumLuminance" },
                                                            { 1881014312, "FLTargetMaximumLuminance" },
                                                            { 1881079848, "FLLuminanceValue" },
                                                            { 1881145384, "LOLuminanceResponseDescription" },
                                                            { 1881210920, "CSWhitePointFlag" },
                                                            { 1881276456, "SQDisplayDeviceTypeCodeSequence" },
                                                            { 1881341992, "SQDisplaySubsystemSequence" },
                                                            { 1881407528, "SQLuminanceResultSequence" },
                                                            { 1881473064, "CSAmbientLightValueSource" },
                                                            { 1881538600, "CSMeasuredCharacteristics" },
                                                            { 1881604136, "SQLuminanceUniformityResultSequence" },
                                                            { 1881669672, "SQVisualEvaluationTestSequence" },
                                                            { 1881735208, "CSTestResult" },
                                                            { 1881800744, "LOTestResultComment" },
                                                            { 1881866280, "CSTestImageValidation" },
                                                            { 1881931816, "SQTestPatternCodeSequence" },
                                                            { 1881997352, "SQMeasurementPatternCodeSequence" },
                                                            { 1882062888, "SQVisualEvaluationMethodCodeSequence" },
                                                            { 2145386536, "URPixelDataProviderURL" },
                                                            { 2415984680, "ULDataPointRows" },
                                                            { 2416050216, "ULDataPointColumns" },
                                                            { 2416115752, "CSSignalDomainColumns" },
                                                            { 2425946152, "USLargestMonochromePixelValue" },
                                                            { 2433220648, "CSDataRepresentation" },
                                                            { 2433744936, "SQPixelMeasuresSequence" },
                                                            { 2435973160, "SQFrameVOILUTSequence" },
                                                            { 2437218344, "SQPixelValueTransformationSequence" },
                                                            { 2452946984, "CSSignalDomainRows" },
                                                            { 2484142120, "FLDisplayFilterPercentage" },
                                                            { 2484404264, "SQFramePixelShiftSequence" },
                                                            { 2484469800, "USSubtractionItemID" },
                                                            { 2485256232, "SQPixelIntensityRelationshipLUTSequence" },
                                                            { 2487418920, "SQFramePixelDataPropertiesSequence" },
                                                            { 2487484456, "CSGeometricalProperties" },
                                                            { 2487549992, "FLGeometricMaximumDistortion" },
                                                            { 2487615528, "CSImageProcessingApplied" },
                                                            { 2488533032, "CSMaskSelectionMode" },
                                                            { 2490630184, "CSLUTFunction" },
                                                            { 2490892328, "FLMaskVisibilityPercentage" },
                                                            { 2499870760, "SQPixelShiftSequence" },
                                                            { 2499936296, "SQRegionPixelShiftSequence" },
                                                            { 2500001832, "SSVerticesOfTheRegion" },
                                                            { 2500132904, "SQMultiFramePresentationSequence" },
                                                            { 2500198440, "USPixelShiftFrameRange" },
                                                            { 2500263976, "USLUTFrameRange" },
                                                            { 2501902376, "DSImageToEquipmentMappingMatrix" },
                                                            { 2503409704, "CSEquipmentCoordinateSystemIdentification" },
                                                            { 655410, "CSStudyStatusID" },
                                                            { 786482, "CSStudyPriorityID" },
                                                            { 1179698, "LOStudyIDIssuer" },
                                                            { 3276850, "DAStudyVerifiedDate" },
                                                            { 3342386, "TMStudyVerifiedTime" },
                                                            { 3407922, "DAStudyReadDate" },
                                                            { 3473458, "TMStudyReadTime" },
                                                            { 268435506, "DAScheduledStudyStartDate" },
                                                            { 268501042, "TMScheduledStudyStartTime" },
                                                            { 269484082, "DAScheduledStudyStopDate" },
                                                            { 269549618, "TMScheduledStudyStopTime" },
                                                            { 270532658, "LOScheduledStudyLocation" },
                                                            { 270598194, "AEScheduledStudyLocationAETitle" },
                                                            { 271581234, "LOReasonForStudy" },
                                                            { 271646770, "SQRequestingPhysicianIdentificationSequence" },
                                                            { 271712306, "PNRequestingPhysician" },
                                                            { 271777842, "LORequestingService" },
                                                            { 271843378, "SQRequestingServiceCodeSequence" },
                                                            { 272629810, "DAStudyArrivalDate" },
                                                            { 272695346, "TMStudyArrivalTime" },
                                                            { 273678386, "DAStudyCompletionDate" },
                                                            { 273743922, "TMStudyCompletionTime" },
                                                            { 274006066, "CSStudyComponentStatusID" },
                                                            { 274726962, "LORequestedProcedureDescription" },
                                                            { 274989106, "SQRequestedProcedureCodeSequence" },
                                                            { 275775538, "LORequestedContrastAgent" },
                                                            { 1073741874, "LTStudyComments" },
                                                            { 262200, "SQReferencedPatientAliasSequence" },
                                                            { 524344, "CSVisitStatusID" },
                                                            { 1048632, "LOAdmissionID" },
                                                            { 1114168, "LOIssuerOfAdmissionID" },
                                                            { 1310776, "SQIssuerOfAdmissionIDSequence" },
                                                            { 1441848, "LORouteOfAdmissions" },
                                                            { 1703992, "DAScheduledAdmissionDate" },
                                                            { 1769528, "TMScheduledAdmissionTime" },
                                                            { 1835064, "DAScheduledDischargeDate" },
                                                            { 1900600, "TMScheduledDischargeTime" },
                                                            { 1966136, "LOScheduledPatientInstitutionResidence" },
                                                            { 2097208, "DAAdmittingDate" },
                                                            { 2162744, "TMAdmittingTime" },
                                                            { 3145784, "DADischargeDate" },
                                                            { 3276856, "TMDischargeTime" },
                                                            { 4194360, "LODischargeDiagnosisDescription" },
                                                            { 4456504, "SQDischargeDiagnosisCodeSequence" },
                                                            { 5242936, "LOSpecialNeeds" },
                                                            { 6291512, "LOServiceEpisodeID" },
                                                            { 6357048, "LOIssuerOfServiceEpisodeID" },
                                                            { 6422584, "LOServiceEpisodeDescription" },
                                                            { 6553656, "SQIssuerOfServiceEpisodeIDSequence" },
                                                            { 16777272, "SQPertinentDocumentsSequence" },
                                                            { 16842808, "SQPertinentResourcesSequence" },
                                                            { 16908344, "LOResourceDescription" },
                                                            { 50331704, "LOCurrentPatientLocation" },
                                                            { 67108920, "LOPatientInstitutionResidence" },
                                                            { 83886136, "LOPatientState" },
                                                            { 84017208, "SQPatientClinicalTrialParticipationSequence" },
                                                            { 1073741880, "LTVisitComments" },
                                                            { 262202, "CSWaveformOriginality" },
                                                            { 327738, "USNumberOfWaveformChannels" },
                                                            { 1048634, "ULNumberOfWaveformSamples" },
                                                            { 1703994, "DSSamplingFrequency" },
                                                            { 2097210, "SHMultiplexGroupLabel" },
                                                            { 33554490, "SQChannelDefinitionSequence" },
                                                            { 33685562, "ISWaveformChannelNumber" },
                                                            { 33751098, "SHChannelLabel" },
                                                            { 33882170, "CSChannelStatus" },
                                                            { 34078778, "SQChannelSourceSequence" },
                                                            { 34144314, "SQChannelSourceModifiersSequence" },
                                                            { 34209850, "SQSourceWaveformSequence" },
                                                            { 34340922, "LOChannelDerivationDescription" },
                                                            { 34603066, "DSChannelSensitivity" },
                                                            { 34668602, "SQChannelSensitivityUnitsSequence" },
                                                            { 34734138, "DSChannelSensitivityCorrectionFactor" },
                                                            { 34799674, "DSChannelBaseline" },
                                                            { 34865210, "DSChannelTimeSkew" },
                                                            { 34930746, "DSChannelSampleSkew" },
                                                            { 35127354, "DSChannelOffset" },
                                                            { 35258426, "USWaveformBitsStored" },
                                                            { 35651642, "DSFilterLowFrequency" },
                                                            { 35717178, "DSFilterHighFrequency" },
                                                            { 35782714, "DSNotchFilterFrequency" },
                                                            { 35848250, "DSNotchFilterBandwidth" },
                                                            { 36700218, "FLWaveformDataDisplayScale" },
                                                            { 36765754, "USWaveformDisplayBackgroundCIELabValue" },
                                                            { 37748794, "SQWaveformPresentationGroupSequence" },
                                                            { 37814330, "USPresentationGroupNumber" },
                                                            { 37879866, "SQChannelDisplaySequence" },
                                                            { 38010938, "USChannelRecommendedDisplayCIELabValue" },
                                                            { 38076474, "FLChannelPosition" },
                                                            { 38142010, "CSDisplayShadingFlag" },
                                                            { 38207546, "FLFractionalChannelDisplayScale" },
                                                            { 38273082, "FLAbsoluteChannelDisplayScale" },
                                                            { 50331706, "SQMultiplexedAudioChannelsDescriptionCodeSequence" },
                                                            { 50397242, "ISChannelIdentificationCode" },
                                                            { 50462778, "CSChannelMode" },
                                                            { 65600, "AEScheduledStationAETitle" },
                                                            { 131136, "DAScheduledProcedureStepStartDate" },
                                                            { 196672, "TMScheduledProcedureStepStartTime" },
                                                            { 262208, "DAScheduledProcedureStepEndDate" },
                                                            { 327744, "TMScheduledProcedureStepEndTime" },
                                                            { 393280, "PNScheduledPerformingPhysicianName" },
                                                            { 458816, "LOScheduledProcedureStepDescription" },
                                                            { 524352, "SQScheduledProtocolCodeSequence" },
                                                            { 589888, "SHScheduledProcedureStepID" },
                                                            { 655424, "SQStageCodeSequence" },
                                                            { 720960, "SQScheduledPerformingPhysicianIdentificationSequence" },
                                                            { 1048640, "SHScheduledStationName" },
                                                            { 1114176, "SHScheduledProcedureStepLocation" },
                                                            { 1179712, "LOPreMedication" },
                                                            { 2097216, "CSScheduledProcedureStepStatus" },
                                                            { 2490432, "SQOrderPlacerIdentifierSequence" },
                                                            { 2555968, "SQOrderFillerIdentifierSequence" },
                                                            { 3211328, "UTLocalNamespaceEntityID" },
                                                            { 3276864, "UTUniversalEntityID" },
                                                            { 3342400, "CSUniversalEntityIDType" },
                                                            { 3473472, "CSIdentifierTypeCode" },
                                                            { 3539008, "SQAssigningFacilitySequence" },
                                                            { 3735616, "SQAssigningJurisdictionCodeSequence" },
                                                            { 3801152, "SQAssigningAgencyOrDepartmentCodeSequence" },
                                                            { 16777280, "SQScheduledProcedureStepSequence" },
                                                            { 35651648, "SQReferencedNonImageCompositeSOPInstanceSequence" },
                                                            { 37814336, "AEPerformedStationAETitle" },
                                                            { 37879872, "SHPerformedStationName" },
                                                            { 37945408, "SHPerformedLocation" },
                                                            { 38010944, "DAPerformedProcedureStepStartDate" },
                                                            { 38076480, "TMPerformedProcedureStepStartTime" },
                                                            { 38797376, "DAPerformedProcedureStepEndDate" },
                                                            { 38862912, "TMPerformedProcedureStepEndTime" },
                                                            { 38928448, "CSPerformedProcedureStepStatus" },
                                                            { 38993984, "SHPerformedProcedureStepID" },
                                                            { 39059520, "LOPerformedProcedureStepDescription" },
                                                            { 39125056, "LOPerformedProcedureTypeDescription" },
                                                            { 39845952, "SQPerformedProtocolCodeSequence" },
                                                            { 39911488, "CSPerformedProtocolType" },
                                                            { 40894528, "SQScheduledStepAttributesSequence" },
                                                            { 41222208, "SQRequestAttributesSequence" },
                                                            { 41943104, "STCommentsOnThePerformedProcedureStep" },
                                                            { 42008640, "SQPerformedProcedureStepDiscontinuationReasonCodeSequence" },
                                                            { 43188288, "SQQuantitySequence" },
                                                            { 43253824, "DSQuantity" },
                                                            { 43319360, "SQMeasuringUnitsSequence" },
                                                            { 43384896, "SQBillingItemSequence" },
                                                            { 50331712, "USTotalTimeOfFluoroscopy" },
                                                            { 50397248, "USTotalNumberOfExposures" },
                                                            { 50462784, "USEntranceDose" },
                                                            { 50528320, "USExposedArea" },
                                                            { 50724928, "DSDistanceSourceToEntrance" },
                                                            { 50790464, "DSDistanceSourceToSupport" },
                                                            { 51249216, "SQExposureDoseSequence" },
                                                            { 51380288, "STCommentsOnRadiationDose" },
                                                            { 51511360, "DSXRayOutput" },
                                                            { 51642432, "DSHalfValueLayer" },
                                                            { 51773504, "DSOrganDose" },
                                                            { 51904576, "CSOrganExposed" },
                                                            { 52428864, "SQBillingProcedureStepSequence" },
                                                            { 52494400, "SQFilmConsumptionSequence" },
                                                            { 52691008, "SQBillingSuppliesAndDevicesSequence" },
                                                            { 53477440, "SQReferencedProcedureStepSequence" },
                                                            { 54526016, "SQPerformedSeriesSequence" },
                                                            { 67108928, "LTCommentsOnTheScheduledProcedureStep" },
                                                            { 71303232, "SQProtocolContextSequence" },
                                                            { 71368768, "SQContentItemModifierSequence" },
                                                            { 83886144, "SQScheduledSpecimenSequence" },
                                                            { 84541504, "LOSpecimenAccessionNumber" },
                                                            { 85065792, "LOContainerIdentifier" },
                                                            { 85131328, "SQIssuerOfTheContainerIdentifierSequence" },
                                                            { 85262400, "SQAlternateContainerIdentifierSequence" },
                                                            { 85459008, "SQContainerTypeCodeSequence" },
                                                            { 85590080, "LOContainerDescription" },
                                                            { 85983296, "SQContainerComponentSequence" },
                                                            { 89129024, "SQSpecimenSequence" },
                                                            { 89194560, "LOSpecimenIdentifier" },
                                                            { 89260096, "SQSpecimenDescriptionSequenceTrial" },
                                                            { 89325632, "STSpecimenDescriptionTrial" },
                                                            { 89391168, "UISpecimenUID" },
                                                            { 89456704, "SQAcquisitionContextSequence" },
                                                            { 89522240, "STAcquisitionContextDescription" },
                                                            { 93978688, "SQSpecimenTypeCodeSequence" },
                                                            { 90177600, "SQSpecimenDescriptionSequence" },
                                                            { 90308672, "SQIssuerOfTheSpecimenIdentifierSequence" },
                                                            { 100663360, "LOSpecimenShortDescription" },
                                                            { 100794432, "UTSpecimenDetailedDescription" },
                                                            { 101711936, "SQSpecimenPreparationSequence" },
                                                            { 101843008, "SQSpecimenPreparationStepContentItemSequence" },
                                                            { 102760512, "SQSpecimenLocalizationContentItemSequence" },
                                                            { 117047360, "LOSlideIdentifier" },
                                                            { 119144512, "SQImageCenterPointCoordinatesSequence" },
                                                            { 120193088, "DSXOffsetInSlideCoordinateSystem" },
                                                            { 121241664, "DSYOffsetInSlideCoordinateSystem" },
                                                            { 122290240, "DSZOffsetInSlideCoordinateSystem" },
                                                            { 148373568, "SQPixelSpacingSequence" },
                                                            { 148504640, "SQCoordinateSystemAxisCodeSequence" },
                                                            { 149553216, "SQMeasurementUnitsCodeSequence" },
                                                            { 167247936, "SQVitalStainCodeSequenceTrial" },
                                                            { 268501056, "SHRequestedProcedureID" },
                                                            { 268566592, "LOReasonForTheRequestedProcedure" },
                                                            { 268632128, "SHRequestedProcedurePriority" },
                                                            { 268697664, "LOPatientTransportArrangements" },
                                                            { 268763200, "LORequestedProcedureLocation" },
                                                            { 268828736, "SHPlacerOrderNumberProcedure" },
                                                            { 268894272, "SHFillerOrderNumberProcedure" },
                                                            { 268959808, "LOConfidentialityCode" },
                                                            { 269025344, "SHReportingPriority" },
                                                            { 269090880, "SQReasonForRequestedProcedureCodeSequence" },
                                                            { 269484096, "PNNamesOfIntendedRecipientsOfResults" },
                                                            { 269549632, "SQIntendedRecipientsOfResultsIdentificationSequence" },
                                                            { 269615168, "SQReasonForPerformedProcedureCodeSequence" },
                                                            { 274726976, "LORequestedProcedureDescriptionTrial" },
                                                            { 285278272, "SQPersonIdentificationCodeSequence" },
                                                            { 285343808, "STPersonAddress" },
                                                            { 285409344, "LOPersonTelephoneNumbers" },
                                                            { 285474880, "LTPersonTelecomInformation" },
                                                            { 335544384, "LTRequestedProcedureComments" },
                                                            { 536936512, "LOReasonForTheImagingServiceRequest" },
                                                            { 537133120, "DAIssueDateOfImagingServiceRequest" },
                                                            { 537198656, "TMIssueTimeOfImagingServiceRequest" },
                                                            { 537264192, "SHPlacerOrderNumberImagingServiceRequestRetired" },
                                                            { 537329728, "SHFillerOrderNumberImagingServiceRequestRetired" },
                                                            { 537395264, "PNOrderEnteredBy" },
                                                            { 537460800, "SHOrderEntererLocation" },
                                                            { 537919552, "SHOrderCallbackPhoneNumber" },
                                                            { 537985088, "LTOrderCallbackTelecomInformation" },
                                                            { 538312768, "LOPlacerOrderNumberImagingServiceRequest" },
                                                            { 538378304, "LOFillerOrderNumberImagingServiceRequest" },
                                                            { 603979840, "LTImagingServiceRequestComments" },
                                                            { 805371968, "LOConfidentialityConstraintOnPatientDataDescription" },
                                                            { 1073807424, "CSGeneralPurposeScheduledProcedureStepStatus" },
                                                            { 1073872960, "CSGeneralPurposePerformedProcedureStepStatus" },
                                                            { 1073938496, "CSGeneralPurposeScheduledProcedureStepPriority" },
                                                            { 1074004032, "SQScheduledProcessingApplicationsCodeSequence" },
                                                            { 1074069568, "DTScheduledProcedureStepStartDateTime" },
                                                            { 1074135104, "CSMultipleCopiesFlag" },
                                                            { 1074200640, "SQPerformedProcessingApplicationsCodeSequence" },
                                                            { 1074331712, "SQHumanPerformerCodeSequence" },
                                                            { 1074790464, "DTScheduledProcedureStepModificationDateTime" },
                                                            { 1074856000, "DTExpectedCompletionDateTime" },
                                                            { 1075118144, "SQResultingGeneralPurposePerformedProcedureStepsSequence" },
                                                            { 1075183680, "SQReferencedGeneralPurposeScheduledProcedureStepSequence" },
                                                            { 1075314752, "SQScheduledWorkitemCodeSequence" },
                                                            { 1075380288, "SQPerformedWorkitemCodeSequence" },
                                                            { 1075839040, "CSInputAvailabilityFlag" },
                                                            { 1075904576, "SQInputInformationSequence" },
                                                            { 1075970112, "SQRelevantInformationSequence" },
                                                            { 1076035648, "UIReferencedGeneralPurposeScheduledProcedureStepTransactionUID" },
                                                            { 1076166720, "SQScheduledStationNameCodeSequence" },
                                                            { 1076232256, "SQScheduledStationClassCodeSequence" },
                                                            { 1076297792, "SQScheduledStationGeographicLocationCodeSequence" },
                                                            { 1076363328, "SQPerformedStationNameCodeSequence" },
                                                            { 1076428864, "SQPerformedStationClassCodeSequence" },
                                                            { 1076887616, "SQPerformedStationGeographicLocationCodeSequence" },
                                                            { 1076953152, "SQRequestedSubsequentWorkitemCodeSequence" },
                                                            { 1077018688, "SQNonDICOMOutputCodeSequence" },
                                                            { 1077084224, "SQOutputInformationSequence" },
                                                            { 1077149760, "SQScheduledHumanPerformersSequence" },
                                                            { 1077215296, "SQActualHumanPerformersSequence" },
                                                            { 1077280832, "LOHumanPerformerOrganization" },
                                                            { 1077346368, "PNHumanPerformerName" },
                                                            { 1077936192, "CSRawDataHandling" },
                                                            { 1078001728, "CSInputReadinessState" },
                                                            { 1078984768, "DTPerformedProcedureStepStartDateTime" },
                                                            { 1079050304, "DTPerformedProcedureStepEndDateTime" },
                                                            { 1079115840, "DTProcedureStepCancellationDateTime" },
                                                            { 1081081920, "SQOutputDestinationSequence" },
                                                            { 1081147456, "SQDICOMStorageSequence" },
                                                            { 1081212992, "SQSTOWRSStorageSequence" },
                                                            { 1081278528, "URStorageURL" },
                                                            { 1081344064, "SQXDSStorageSequence" },
                                                            { 2197946432, "DSEntranceDoseInmGy" },
                                                            { 2425487424, "SQParametricMapFrameTypeSequence" },
                                                            { 2425618496, "SQReferencedImageRealWorldValueMappingSequence" },
                                                            { 2425749568, "SQRealWorldValueMappingSequence" },
                                                            { 2425880640, "SQPixelValueMappingCodeSequence" },
                                                            { 2450522176, "SHLUTLabel" },
                                                            { 2450587712, "USRealWorldValueLastValueMapped" },
                                                            { 2450653248, "FDRealWorldValueLUTData" },
                                                            { 2450718784, "FDDoubleFloatRealWorldValueLastValueMapped" },
                                                            { 2450784320, "FDDoubleFloatRealWorldValueFirstValueMapped" },
                                                            { 2450915392, "USRealWorldValueFirstValueMapped" },
                                                            { 2451570752, "SQQuantityDefinitionSequence" },
                                                            { 2451832896, "FDRealWorldValueIntercept" },
                                                            { 2451898432, "FDRealWorldValueSlope" },
                                                            { 2684813376, "CSFindingsFlagTrial" },
                                                            { 2685403200, "CSRelationshipType" },
                                                            { 2686451776, "SQFindingsSequenceTrial" },
                                                            { 2686517312, "UIFindingsGroupUIDTrial" },
                                                            { 2686582848, "UIReferencedFindingsGroupUIDTrial" },
                                                            { 2686648384, "DAFindingsGroupRecordingDateTrial" },
                                                            { 2686713920, "TMFindingsGroupRecordingTimeTrial" },
                                                            { 2686844992, "SQFindingsSourceCategoryCodeSequenceTrial" },
                                                            { 2686910528, "LOVerifyingOrganization" },
                                                            { 2686976064, "SQDocumentingOrganizationIdentifierCodeSequenceTrial" },
                                                            { 2687500352, "DTVerificationDateTime" },
                                                            { 2687631424, "DTObservationDateTime" },
                                                            { 2688548928, "CSValueType" },
                                                            { 2688745536, "SQConceptNameCodeSequence" },
                                                            { 2689007680, "LOMeasurementPrecisionDescriptionTrial" },
                                                            { 2689597504, "CSContinuityOfContent" },
                                                            { 2690056256, "CSUrgencyOrPriorityAlertsTrial" },
                                                            { 2690646080, "LOSequencingIndicatorTrial" },
                                                            { 2691039296, "SQDocumentIdentifierCodeSequenceTrial" },
                                                            { 2691104832, "PNDocumentAuthorTrial" },
                                                            { 2691170368, "SQDocumentAuthorIdentifierCodeSequenceTrial" },
                                                            { 2691694656, "SQIdentifierCodeSequenceTrial" },
                                                            { 2691891264, "SQVerifyingObserverSequence" },
                                                            { 2691956800, "OBObjectBinaryIdentifierTrial" },
                                                            { 2692022336, "PNVerifyingObserverName" },
                                                            { 2692087872, "SQDocumentingObserverIdentifierCodeSequenceTrial" },
                                                            { 2692218944, "SQAuthorObserverSequence" },
                                                            { 2692350016, "SQParticipantSequence" },
                                                            { 2692481088, "SQCustodialOrganizationSequence" },
                                                            { 2692743232, "CSParticipationType" },
                                                            { 2692874304, "DTParticipationDateTime" },
                                                            { 2693005376, "CSObserverType" },
                                                            { 2693070912, "SQProcedureIdentifierCodeSequenceTrial" },
                                                            { 2693267520, "SQVerifyingObserverIdentificationCodeSequence" },
                                                            { 2693333056, "OBObjectDirectoryBinaryIdentifierTrial" },
                                                            { 2693791808, "SQEquivalentCDADocumentSequence" },
                                                            { 2695888960, "USReferencedWaveformChannels" },
                                                            { 2702180416, "DADateOfDocumentOrVerbalTransactionTrial" },
                                                            { 2702311488, "TMTimeOfDocumentCreationOrVerbalTransactionTrial" },
                                                            { 2703228992, "DTDateTime" },
                                                            { 2703294528, "DADate" },
                                                            { 2703360064, "TMTime" },
                                                            { 2703425600, "PNPersonName" },
                                                            { 2703491136, "UIUID" },
                                                            { 2703556672, "CSReportStatusIDTrial" },
                                                            { 2704277568, "CSTemporalRangeType" },
                                                            { 2704408640, "ULReferencedSamplePositions" },
                                                            { 2704670784, "USReferencedFrameNumbers" },
                                                            { 2704801856, "DSReferencedTimeOffsets" },
                                                            { 2704932928, "DTReferencedDateTime" },
                                                            { 2707423296, "UTTextValue" },
                                                            { 2707488832, "FDFloatingPointValue" },
                                                            { 2707554368, "SLRationalNumeratorValue" },
                                                            { 2707619904, "ULRationalDenominatorValue" },
                                                            { 2707882048, "SQObservationCategoryCodeSequenceTrial" },
                                                            { 2707947584, "SQConceptCodeSequence" },
                                                            { 2708078656, "STBibliographicCitationTrial" },
                                                            { 2708471872, "SQPurposeOfReferenceCodeSequence" },
                                                            { 2708537408, "UIObservationUID" },
                                                            { 2708602944, "UIReferencedObservationUIDTrial" },
                                                            { 2708668480, "CSReferencedObservationClassTrial" },
                                                            { 2708734016, "CSReferencedObjectObservationClassTrial" },
                                                            { 2709520448, "USAnnotationGroupNumber" },
                                                            { 2710700096, "DAObservationDateTrial" },
                                                            { 2710765632, "TMObservationTimeTrial" },
                                                            { 2710831168, "CSMeasurementAutomationTrial" },
                                                            { 2710896704, "SQModifierCodeSequence" },
                                                            { 2720268352, "STIdentificationDescriptionTrial" },
                                                            { 2727346240, "CSCoordinatesSetGeometricTypeTrial" },
                                                            { 2727739456, "SQAlgorithmCodeSequenceTrial" },
                                                            { 2727804992, "STAlgorithmDescriptionTrial" },
                                                            { 2728001600, "SLPixelCoordinatesSetTrial" },
                                                            { 2734686272, "SQMeasuredValueSequence" },
                                                            { 2734751808, "SQNumericValueQualifierCodeSequence" },
                                                            { 2735145024, "PNCurrentObserverTrial" },
                                                            { 2735341632, "DSNumericValue" },
                                                            { 2735931456, "SQReferencedAccessionSequenceTrial" },
                                                            { 2738487360, "STReportStatusCommentTrial" },
                                                            { 2738880576, "SQProcedureContextSequenceTrial" },
                                                            { 2740060224, "PNVerbalSourceTrial" },
                                                            { 2740125760, "STAddressTrial" },
                                                            { 2740191296, "LOTelephoneNumberTrial" },
                                                            { 2740453440, "SQVerbalSourceIdentifierCodeSequenceTrial" },
                                                            { 2740977728, "SQPredecessorDocumentsSequence" },
                                                            { 2742026304, "SQReferencedRequestSequence" },
                                                            { 2742157376, "SQPerformedProcedureCodeSequence" },
                                                            { 2742353984, "SQCurrentRequestedProcedureEvidenceSequence" },
                                                            { 2743074880, "SQReportDetailSequenceTrial" },
                                                            { 2743402560, "SQPertinentOtherEvidenceSequence" },
                                                            { 2744123456, "SQHL7StructuredDocumentReferenceSequence" },
                                                            { 2751594560, "UIObservationSubjectUIDTrial" },
                                                            { 2751660096, "CSObservationSubjectClassTrial" },
                                                            { 2751725632, "SQObservationSubjectTypeCodeSequenceTrial" },
                                                            { 2760966208, "CSCompletionFlag" },
                                                            { 2761031744, "LOCompletionFlagDescription" },
                                                            { 2761097280, "CSVerificationFlag" },
                                                            { 2761162816, "CSArchiveRequested" },
                                                            { 2761293888, "CSPreliminaryFlag" },
                                                            { 2768502848, "SQContentTemplateSequence" },
                                                            { 2770665536, "SQIdenticalDocumentsSequence" },
                                                            { 2785017920, "CSObservationSubjectContextFlagTrial" },
                                                            { 2785083456, "CSObserverContextFlagTrial" },
                                                            { 2785214528, "CSProcedureContextFlagTrial" },
                                                            { 2804940864, "SQContentSequence" },
                                                            { 2805006400, "SQRelationshipSequenceTrial" },
                                                            { 2805071936, "SQRelationshipTypeCodeSequenceTrial" },
                                                            { 2806251584, "SQLanguageCodeSequenceTrial" },
                                                            { 2844917824, "STUniformResourceLocatorTrial" },
                                                            { 2954887232, "SQWaveformAnnotationSequence" },
                                                            { 3674210368, "CSTemplateIdentifier" },
                                                            { 3674603584, "DTTemplateVersion" },
                                                            { 3674669120, "DTTemplateLocalVersion" },
                                                            { 3674931264, "CSTemplateExtensionFlag" },
                                                            { 3674996800, "UITemplateExtensionOrganizationUID" },
                                                            { 3675062336, "UITemplateExtensionCreatorUID" },
                                                            { 3681747008, "ULReferencedContentItemIdentifier" },
                                                            { 3758161984, "STHL7InstanceIdentifier" },
                                                            { 3758358592, "DTHL7DocumentEffectiveTime" },
                                                            { 3758489664, "SQHL7DocumentTypeCodeSequence" },
                                                            { 3758620736, "SQDocumentClassCodeSequence" },
                                                            { 3759145024, "URRetrieveURI" },
                                                            { 3759210560, "UIRetrieveLocationUID" },
                                                            { 3760193600, "CSTypeOfInstances" },
                                                            { 3760259136, "SQDICOMRetrievalSequence" },
                                                            { 3760324672, "SQDICOMMediaRetrievalSequence" },
                                                            { 3760390208, "SQWADORetrievalSequence" },
                                                            { 3760455744, "SQXDSRetrievalSequence" },
                                                            { 3760521280, "SQWADORSRetrievalSequence" },
                                                            { 3761242176, "UIRepositoryUniqueID" },
                                                            { 3761307712, "UIHomeCommunityID" },
                                                            { 1048642, "STDocumentTitle" },
                                                            { 1114178, "OBEncapsulatedDocument" },
                                                            { 1179714, "LOMIMETypeOfEncapsulatedDocument" },
                                                            { 1245250, "SQSourceInstanceSequence" },
                                                            { 1310786, "LOListOfMIMETypes" },
                                                            { 65604, "STProductPackageIdentifier" },
                                                            { 131140, "CSSubstanceAdministrationApproval" },
                                                            { 196676, "LTApprovalStatusFurtherDescription" },
                                                            { 262212, "DTApprovalStatusDateTime" },
                                                            { 458820, "SQProductTypeCodeSequence" },
                                                            { 524356, "LOProductName" },
                                                            { 589892, "LTProductDescription" },
                                                            { 655428, "LOProductLotIdentifier" },
                                                            { 720964, "DTProductExpirationDateTime" },
                                                            { 1048644, "DTSubstanceAdministrationDateTime" },
                                                            { 1114180, "LOSubstanceAdministrationNotes" },
                                                            { 1179716, "LOSubstanceAdministrationDeviceID" },
                                                            { 1245252, "SQProductParameterSequence" },
                                                            { 1638468, "SQSubstanceAdministrationParameterSequence" },
                                                            { 1179718, "LOLensDescription" },
                                                            { 1310790, "SQRightLensSequence" },
                                                            { 1376326, "SQLeftLensSequence" },
                                                            { 1441862, "SQUnspecifiedLateralityLensSequence" },
                                                            { 1572934, "SQCylinderSequence" },
                                                            { 2621510, "SQPrismSequence" },
                                                            { 3145798, "FDHorizontalPrismPower" },
                                                            { 3276870, "CSHorizontalPrismBase" },
                                                            { 3407942, "FDVerticalPrismPower" },
                                                            { 3539014, "CSVerticalPrismBase" },
                                                            { 3670086, "CSLensSegmentType" },
                                                            { 4194374, "FDOpticalTransmittance" },
                                                            { 4325446, "FDChannelWidth" },
                                                            { 4456518, "FDPupilSize" },
                                                            { 4587590, "FDCornealSize" },
                                                            { 5242950, "SQAutorefractionRightEyeSequence" },
                                                            { 5374022, "SQAutorefractionLeftEyeSequence" },
                                                            { 6291526, "FDDistancePupillaryDistance" },
                                                            { 6422598, "FDNearPupillaryDistance" },
                                                            { 6488134, "FDIntermediatePupillaryDistance" },
                                                            { 6553670, "FDOtherPupillaryDistance" },
                                                            { 7340102, "SQKeratometryRightEyeSequence" },
                                                            { 7405638, "SQKeratometryLeftEyeSequence" },
                                                            { 7602246, "SQSteepKeratometricAxisSequence" },
                                                            { 7667782, "FDRadiusOfCurvature" },
                                                            { 7733318, "FDKeratometricPower" },
                                                            { 7798854, "FDKeratometricAxis" },
                                                            { 8388678, "SQFlatKeratometricAxisSequence" },
                                                            { 9568326, "CSBackgroundColor" },
                                                            { 9699398, "CSOptotype" },
                                                            { 9764934, "CSOptotypePresentation" },
                                                            { 9896006, "SQSubjectiveRefractionRightEyeSequence" },
                                                            { 9961542, "SQSubjectiveRefractionLeftEyeSequence" },
                                                            { 16777286, "SQAddNearSequence" },
                                                            { 16842822, "SQAddIntermediateSequence" },
                                                            { 16908358, "SQAddOtherSequence" },
                                                            { 17039430, "FDAddPower" },
                                                            { 17170502, "FDViewingDistance" },
                                                            { 18939974, "SQVisualAcuityTypeCodeSequence" },
                                                            { 19005510, "SQVisualAcuityRightEyeSequence" },
                                                            { 19071046, "SQVisualAcuityLeftEyeSequence" },
                                                            { 19136582, "SQVisualAcuityBothEyesOpenSequence" },
                                                            { 19202118, "CSViewingDistanceType" },
                                                            { 20250694, "SSVisualAcuityModifiers" },
                                                            { 20381766, "FDDecimalVisualAcuity" },
                                                            { 20512838, "LOOptotypeDetailedDefinition" },
                                                            { 21299270, "SQReferencedRefractiveMeasurementsSequence" },
                                                            { 21364806, "FDSpherePower" },
                                                            { 21430342, "FDCylinderPower" },
                                                            { 33620038, "CSCornealTopographySurface" },
                                                            { 33685574, "FLCornealVertexLocation" },
                                                            { 33751110, "FLPupilCentroidXCoordinate" },
                                                            { 33816646, "FLPupilCentroidYCoordinate" },
                                                            { 33882182, "FLEquivalentPupilRadius" },
                                                            { 34013254, "SQCornealTopographyMapTypeCodeSequence" },
                                                            { 34078790, "ISVerticesOfTheOutlineOfPupil" },
                                                            { 34603078, "SQCornealTopographyMappingNormalsSequence" },
                                                            { 34668614, "SQMaximumCornealCurvatureSequence" },
                                                            { 34734150, "FLMaximumCornealCurvature" },
                                                            { 34799686, "FLMaximumCornealCurvatureLocation" },
                                                            { 34930758, "SQMinimumKeratometricSequence" },
                                                            { 35127366, "SQSimulatedKeratometricCylinderSequence" },
                                                            { 35651654, "FLAverageCornealPower" },
                                                            { 35913798, "FLCornealISValue" },
                                                            { 36110406, "FLAnalyzedArea" },
                                                            { 36700230, "FLSurfaceRegularityIndex" },
                                                            { 36831302, "FLSurfaceAsymmetryIndex" },
                                                            { 36962374, "FLCornealEccentricityIndex" },
                                                            { 37093446, "FLKeratoconusPredictionIndex" },
                                                            { 37224518, "FLDecimalPotentialVisualAcuity" },
                                                            { 37879878, "CSCornealTopographyMapQualityEvaluation" },
                                                            { 38010950, "SQSourceImageCornealProcessedDataSequence" },
                                                            { 38207558, "FLCornealPointLocation" },
                                                            { 38273094, "CSCornealPointEstimated" },
                                                            { 38338630, "FLAxialPower" },
                                                            { 38797382, "FLTangentialPower" },
                                                            { 38862918, "FLRefractivePower" },
                                                            { 38928454, "FLRelativeElevation" },
                                                            { 38993990, "FLCornealWavefront" },
                                                            { 65608, "FLImagedVolumeWidth" },
                                                            { 131144, "FLImagedVolumeHeight" },
                                                            { 196680, "FLImagedVolumeDepth" },
                                                            { 393288, "ULTotalPixelMatrixColumns" },
                                                            { 458824, "ULTotalPixelMatrixRows" },
                                                            { 524360, "SQTotalPixelMatrixOriginSequence" },
                                                            { 1048648, "CSSpecimenLabelInImage" },
                                                            { 1114184, "CSFocusMethod" },
                                                            { 1179720, "CSExtendedDepthOfField" },
                                                            { 1245256, "USNumberOfFocalPlanes" },
                                                            { 1310792, "FLDistanceBetweenFocalPlanes" },
                                                            { 1376328, "USRecommendedAbsentPixelCIELabValue" },
                                                            { 16777288, "SQIlluminatorTypeCodeSequence" },
                                                            { 16908360, "DSImageOrientationSlide" },
                                                            { 17104968, "SQOpticalPathSequence" },
                                                            { 17170504, "SHOpticalPathIdentifier" },
                                                            { 17236040, "STOpticalPathDescription" },
                                                            { 17301576, "SQIlluminationColorCodeSequence" },
                                                            { 17825864, "SQSpecimenReferenceSequence" },
                                                            { 17891400, "DSCondenserLensPower" },
                                                            { 17956936, "DSObjectiveLensPower" },
                                                            { 18022472, "DSObjectiveLensNumericalAperture" },
                                                            { 18874440, "SQPaletteColorLookupTableSequence" },
                                                            { 33554504, "SQReferencedImageNavigationSequence" },
                                                            { 33620040, "USTopLeftHandCornerOfLocalizerArea" },
                                                            { 33685576, "USBottomRightHandCornerOfLocalizerArea" },
                                                            { 34013256, "SQOpticalPathIdentificationSequence" },
                                                            { 35258440, "SQPlanePositionSlideSequence" },
                                                            { 35520584, "SLColumnPositionInTotalImagePixelMatrix" },
                                                            { 35586120, "SLRowPositionInTotalImagePixelMatrix" },
                                                            { 50397256, "CSPixelOriginInterpretation" },
                                                            { 262224, "CSCalibrationImage" },
                                                            { 1048656, "SQDeviceSequence" },
                                                            { 1179728, "SQContainerComponentTypeCodeSequence" },
                                                            { 1245264, "FDContainerComponentThickness" },
                                                            { 1310800, "DSDeviceLength" },
                                                            { 1376336, "FDContainerComponentWidth" },
                                                            { 1441872, "DSDeviceDiameter" },
                                                            { 1507408, "CSDeviceDiameterUnits" },
                                                            { 1572944, "DSDeviceVolume" },
                                                            { 1638480, "DSInterMarkerDistance" },
                                                            { 1704016, "CSContainerComponentMaterial" },
                                                            { 1769552, "LOContainerComponentID" },
                                                            { 1835088, "FDContainerComponentLength" },
                                                            { 1900624, "FDContainerComponentDiameter" },
                                                            { 1966160, "LOContainerComponentDescription" },
                                                            { 2097232, "LODeviceDescription" },
                                                            { 65618, "FLContrastBolusIngredientPercentByVolume" },
                                                            { 131154, "FDOCTFocalDistance" },
                                                            { 196690, "FDBeamSpotSize" },
                                                            { 262226, "FDEffectiveRefractiveIndex" },
                                                            { 393298, "CSOCTAcquisitionDomain" },
                                                            { 458834, "FDOCTOpticalCenterWavelength" },
                                                            { 524370, "FDAxialResolution" },
                                                            { 589906, "FDRangingDepth" },
                                                            { 1114194, "FDALineRate" },
                                                            { 1179730, "USALinesPerFrame" },
                                                            { 1245266, "FDCatheterRotationalRate" },
                                                            { 1310802, "FDALinePixelSpacing" },
                                                            { 1441874, "SQModeOfPercutaneousAccessSequence" },
                                                            { 2424914, "SQIntravascularOCTFrameTypeSequence" },
                                                            { 2490450, "CSOCTZOffsetApplied" },
                                                            { 2555986, "SQIntravascularFrameContentSequence" },
                                                            { 2621522, "FDIntravascularLongitudinalDistance" },
                                                            { 2687058, "SQIntravascularOCTFrameContentSequence" },
                                                            { 3145810, "SSOCTZOffsetCorrection" },
                                                            { 3211346, "CSCatheterDirectionOfRotation" },
                                                            { 3342418, "FDSeamLineLocation" },
                                                            { 3407954, "FDFirstALineLocation" },
                                                            { 3539026, "USSeamLineIndex" },
                                                            { 3670098, "USNumberOfPaddedALines" },
                                                            { 3735634, "CSInterpolationType" },
                                                            { 3801170, "CSRefractiveIndexApplied" },
                                                            { 1048660, "USEnergyWindowVector" },
                                                            { 1114196, "USNumberOfEnergyWindows" },
                                                            { 1179732, "SQEnergyWindowInformationSequence" },
                                                            { 1245268, "SQEnergyWindowRangeSequence" },
                                                            { 1310804, "DSEnergyWindowLowerLimit" },
                                                            { 1376340, "DSEnergyWindowUpperLimit" },
                                                            { 1441876, "SQRadiopharmaceuticalInformationSequence" },
                                                            { 1507412, "ISResidualSyringeCounts" },
                                                            { 1572948, "SHEnergyWindowName" },
                                                            { 2097236, "USDetectorVector" },
                                                            { 2162772, "USNumberOfDetectors" },
                                                            { 2228308, "SQDetectorInformationSequence" },
                                                            { 3145812, "USPhaseVector" },
                                                            { 3211348, "USNumberOfPhases" },
                                                            { 3276884, "SQPhaseInformationSequence" },
                                                            { 3342420, "USNumberOfFramesInPhase" },
                                                            { 3539028, "ISPhaseDelay" },
                                                            { 3670100, "ISPauseBetweenFrames" },
                                                            { 3735636, "CSPhaseDescription" },
                                                            { 5242964, "USRotationVector" },
                                                            { 5308500, "USNumberOfRotations" },
                                                            { 5374036, "SQRotationInformationSequence" },
                                                            { 5439572, "USNumberOfFramesInRotation" },
                                                            { 6291540, "USRRIntervalVector" },
                                                            { 6357076, "USNumberOfRRIntervals" },
                                                            { 6422612, "SQGatedInformationSequence" },
                                                            { 6488148, "SQDataInformationSequence" },
                                                            { 7340116, "USTimeSlotVector" },
                                                            { 7405652, "USNumberOfTimeSlots" },
                                                            { 7471188, "SQTimeSlotInformationSequence" },
                                                            { 7536724, "DSTimeSlotTime" },
                                                            { 8388692, "USSliceVector" },
                                                            { 8454228, "USNumberOfSlices" },
                                                            { 9437268, "USAngularViewVector" },
                                                            { 16777300, "USTimeSliceVector" },
                                                            { 16842836, "USNumberOfTimeSlices" },
                                                            { 33554516, "DSStartAngle" },
                                                            { 33685588, "CSTypeOfDetectorMotion" },
                                                            { 34603092, "ISTriggerVector" },
                                                            { 34668628, "USNumberOfTriggersInPhase" },
                                                            { 35651668, "SQViewCodeSequence" },
                                                            { 35782740, "SQViewModifierCodeSequence" },
                                                            { 50331732, "SQRadionuclideCodeSequence" },
                                                            { 50462804, "SQAdministrationRouteCodeSequence" },
                                                            { 50593876, "SQRadiopharmaceuticalCodeSequence" },
                                                            { 50724948, "SQCalibrationDataSequence" },
                                                            { 50856020, "USEnergyWindowNumber" },
                                                            { 67108948, "SHImageID" },
                                                            { 68157524, "SQPatientOrientationCodeSequence" },
                                                            { 68288596, "SQPatientOrientationModifierCodeSequence" },
                                                            { 68419668, "SQPatientGantryRelationshipCodeSequence" },
                                                            { 83886164, "CSSliceProgressionDirection" },
                                                            { 83951700, "CSScanProgressionDirection" },
                                                            { 268435540, "CSSeriesType" },
                                                            { 268501076, "CSUnits" },
                                                            { 268566612, "CSCountsSource" },
                                                            { 268697684, "CSReprojectionMethod" },
                                                            { 268828756, "CSSUVType" },
                                                            { 285212756, "CSRandomsCorrectionMethod" },
                                                            { 285278292, "LOAttenuationCorrectionMethod" },
                                                            { 285343828, "CSDecayCorrection" },
                                                            { 285409364, "LOReconstructionMethod" },
                                                            { 285474900, "LODetectorLinesOfResponseUsed" },
                                                            { 285540436, "LOScatterCorrectionMethod" },
                                                            { 301989972, "DSAxialAcceptance" },
                                                            { 302055508, "ISAxialMash" },
                                                            { 302121044, "ISTransverseMash" },
                                                            { 302186580, "DSDetectorElementSize" },
                                                            { 303038548, "DSCoincidenceWindowWidth" },
                                                            { 304087124, "CSSecondaryCountsType" },
                                                            { 318767188, "DSFrameReferenceTime" },
                                                            { 319815764, "ISPrimaryPromptsCountsAccumulated" },
                                                            { 319881300, "ISSecondaryCountsAccumulated" },
                                                            { 320864340, "DSSliceSensitivityFactor" },
                                                            { 320929876, "DSDecayFactor" },
                                                            { 320995412, "DSDoseCalibrationFactor" },
                                                            { 321060948, "DSScatterFractionFactor" },
                                                            { 321126484, "DSDeadTimeFactor" },
                                                            { 321912916, "USImageIndex" },
                                                            { 335544404, "CSCountsIncluded" },
                                                            { 335609940, "CSDeadTimeCorrectionFlag" },
                                                            { 805306464, "SQHistogramSequence" },
                                                            { 805437536, "USHistogramNumberOfBins" },
                                                            { 805568608, "USHistogramFirstBinValue" },
                                                            { 805699680, "USHistogramLastBinValue" },
                                                            { 805830752, "USHistogramBinWidth" },
                                                            { 806355040, "LOHistogramExplanation" },
                                                            { 807403616, "ULHistogramData" },
                                                            { 65634, "CSSegmentationType" },
                                                            { 131170, "SQSegmentSequence" },
                                                            { 196706, "SQSegmentedPropertyCategoryCodeSequence" },
                                                            { 262242, "USSegmentNumber" },
                                                            { 327778, "LOSegmentLabel" },
                                                            { 393314, "STSegmentDescription" },
                                                            { 458850, "SQSegmentationAlgorithmIdentificationSequence" },
                                                            { 524386, "CSSegmentAlgorithmType" },
                                                            { 589922, "LOSegmentAlgorithmName" },
                                                            { 655458, "SQSegmentIdentificationSequence" },
                                                            { 720994, "USReferencedSegmentNumber" },
                                                            { 786530, "USRecommendedDisplayGrayscaleValue" },
                                                            { 852066, "USRecommendedDisplayCIELabValue" },
                                                            { 917602, "USMaximumFractionalValue" },
                                                            { 983138, "SQSegmentedPropertyTypeCodeSequence" },
                                                            { 1048674, "CSSegmentationFractionalType" },
                                                            { 1114210, "SQSegmentedPropertyTypeModifierCodeSequence" },
                                                            { 1179746, "SQUsedSegmentsSequence" },
                                                            { 2097250, "UTTrackingID" },
                                                            { 2162786, "UITrackingUID" },
                                                            { 131172, "SQDeformableRegistrationSequence" },
                                                            { 196708, "UISourceFrameOfReferenceUID" },
                                                            { 327780, "SQDeformableRegistrationGridSequence" },
                                                            { 458852, "ULGridDimensions" },
                                                            { 524388, "FDGridResolution" },
                                                            { 589924, "OFVectorGridData" },
                                                            { 983140, "SQPreDeformationMatrixRegistrationSequence" },
                                                            { 1048676, "SQPostDeformationMatrixRegistrationSequence" },
                                                            { 65638, "ULNumberOfSurfaces" },
                                                            { 131174, "SQSurfaceSequence" },
                                                            { 196710, "ULSurfaceNumber" },
                                                            { 262246, "LTSurfaceComments" },
                                                            { 589926, "CSSurfaceProcessing" },
                                                            { 655462, "FLSurfaceProcessingRatio" },
                                                            { 720998, "LOSurfaceProcessingDescription" },
                                                            { 786534, "FLRecommendedPresentationOpacity" },
                                                            { 852070, "CSRecommendedPresentationType" },
                                                            { 917606, "CSFiniteVolume" },
                                                            { 1048678, "CSManifold" },
                                                            { 1114214, "SQSurfacePointsSequence" },
                                                            { 1179750, "SQSurfacePointsNormalsSequence" },
                                                            { 1245286, "SQSurfaceMeshPrimitivesSequence" },
                                                            { 1376358, "ULNumberOfSurfacePoints" },
                                                            { 1441894, "OFPointCoordinatesData" },
                                                            { 1507430, "FLPointPositionAccuracy" },
                                                            { 1572966, "FLMeanPointDistance" },
                                                            { 1638502, "FLMaximumPointDistance" },
                                                            { 1704038, "FLPointsBoundingBoxCoordinates" },
                                                            { 1769574, "FLAxisOfRotation" },
                                                            { 1835110, "FLCenterOfRotation" },
                                                            { 1966182, "ULNumberOfVectors" },
                                                            { 2031718, "USVectorDimensionality" },
                                                            { 2097254, "FLVectorAccuracy" },
                                                            { 2162790, "OFVectorCoordinateData" },
                                                            { 2293862, "OWTrianglePointIndexList" },
                                                            { 2359398, "OWEdgePointIndexList" },
                                                            { 2424934, "OWVertexPointIndexList" },
                                                            { 2490470, "SQTriangleStripSequence" },
                                                            { 2556006, "SQTriangleFanSequence" },
                                                            { 2621542, "SQLineSequence" },
                                                            { 2687078, "OWPrimitivePointIndexList" },
                                                            { 2752614, "ULSurfaceCount" },
                                                            { 2818150, "SQReferencedSurfaceSequence" },
                                                            { 2883686, "ULReferencedSurfaceNumber" },
                                                            { 2949222, "SQSegmentSurfaceGenerationAlgorithmIdentificationSequence" },
                                                            { 3014758, "SQSegmentSurfaceSourceInstanceSequence" },
                                                            { 3080294, "SQAlgorithmFamilyCodeSequence" },
                                                            { 3145830, "SQAlgorithmNameCodeSequence" },
                                                            { 3211366, "LOAlgorithmVersion" },
                                                            { 3276902, "LTAlgorithmParameters" },
                                                            { 3407974, "SQFacetSequence" },
                                                            { 3473510, "SQSurfaceProcessingAlgorithmIdentificationSequence" },
                                                            { 3539046, "LOAlgorithmName" },
                                                            { 3604582, "FLRecommendedPointRadius" },
                                                            { 3670118, "FLRecommendedLineThickness" },
                                                            { 4194406, "OLLongPrimitivePointIndexList" },
                                                            { 4259942, "OLLongTrianglePointIndexList" },
                                                            { 4325478, "OLLongEdgePointIndexList" },
                                                            { 4391014, "OLLongVertexPointIndexList" },
                                                            { 16842854, "SQTrackSetSequence" },
                                                            { 16908390, "SQTrackSequence" },
                                                            { 16973926, "OWRecommendedDisplayCIELabValueList" },
                                                            { 17039462, "SQTrackingAlgorithmIdentificationSequence" },
                                                            { 17104998, "ULTrackSetNumber" },
                                                            { 17170534, "LOTrackSetLabel" },
                                                            { 17236070, "UTTrackSetDescription" },
                                                            { 17301606, "SQTrackSetAnatomicalTypeCodeSequence" },
                                                            { 18940006, "SQMeasurementsSequence" },
                                                            { 19136614, "SQTrackSetStatisticsSequence" },
                                                            { 19202150, "OFFloatingPointValues" },
                                                            { 19464294, "OLTrackPointIndexList" },
                                                            { 19923046, "SQTrackStatisticsSequence" },
                                                            { 20054118, "SQMeasurementValuesSequence" },
                                                            { 20119654, "SQDiffusionAcquisitionCodeSequence" },
                                                            { 20185190, "SQDiffusionModelCodeSequence" },
                                                            { 1645215848, "LOImplantSize" },
                                                            { 1646329960, "LOImplantTemplateVersion" },
                                                            { 1646395496, "SQReplacedImplantTemplateSequence" },
                                                            { 1646461032, "CSImplantType" },
                                                            { 1646526568, "SQDerivationImplantTemplateSequence" },
                                                            { 1646592104, "SQOriginalImplantTemplateSequence" },
                                                            { 1646657640, "DTEffectiveDateTime" },
                                                            { 1647313000, "SQImplantTargetAnatomySequence" },
                                                            { 1650458728, "SQInformationFromManufacturerSequence" },
                                                            { 1650786408, "SQNotificationFromManufacturerSequence" },
                                                            { 1651507304, "DTInformationIssueDateTime" },
                                                            { 1652555880, "STInformationSummary" },
                                                            { 1654653032, "SQImplantRegulatoryDisapprovalCodeSequence" },
                                                            { 1654980712, "FDOverallTemplateSpatialTolerance" },
                                                            { 1656750184, "SQHPGLDocumentSequence" },
                                                            { 1657798760, "USHPGLDocumentID" },
                                                            { 1658126440, "LOHPGLDocumentLabel" },
                                                            { 1658847336, "SQViewOrientationCodeSequence" },
                                                            { 1659895912, "FDViewOrientationModifier" },
                                                            { 1660026984, "FDHPGLDocumentScaling" },
                                                            { 1660944488, "OBHPGLDocument" },
                                                            { 1661993064, "USHPGLContourPenNumber" },
                                                            { 1663041640, "SQHPGLPenSequence" },
                                                            { 1664090216, "USHPGLPenNumber" },
                                                            { 1665138792, "LOHPGLPenLabel" },
                                                            { 1665466472, "STHPGLPenDescription" },
                                                            { 1665532008, "FDRecommendedRotationPoint" },
                                                            { 1665597544, "FDBoundingRectangle" },
                                                            { 1666187368, "USImplantTemplate3DModelSurfaceNumber" },
                                                            { 1667235944, "SQSurfaceModelDescriptionSequence" },
                                                            { 1669333096, "LOSurfaceModelLabel" },
                                                            { 1670381672, "FDSurfaceModelScalingFactor" },
                                                            { 1671430248, "SQMaterialsCodeSequence" },
                                                            { 1671692392, "SQCoatingMaterialsCodeSequence" },
                                                            { 1671954536, "SQImplantTypeCodeSequence" },
                                                            { 1672216680, "SQFixationMethodCodeSequence" },
                                                            { 1672478824, "SQMatingFeatureSetsSequence" },
                                                            { 1673527400, "USMatingFeatureSetID" },
                                                            { 1674575976, "LOMatingFeatureSetLabel" },
                                                            { 1675624552, "SQMatingFeatureSequence" },
                                                            { 1676673128, "USMatingFeatureID" },
                                                            { 1677721704, "SQMatingFeatureDegreeOfFreedomSequence" },
                                                            { 1678770280, "USDegreeOfFreedomID" },
                                                            { 1679818856, "CSDegreeOfFreedomType" },
                                                            { 1680867432, "SQTwoDMatingFeatureCoordinatesSequence" },
                                                            { 1681916008, "USReferencedHPGLDocumentID" },
                                                            { 1682964584, "FDTwoDMatingPoint" },
                                                            { 1684013160, "FDTwoDMatingAxes" },
                                                            { 1685061736, "SQTwoDDegreeOfFreedomSequence" },
                                                            { 1687158888, "FDThreeDDegreeOfFreedomAxis" },
                                                            { 1688207464, "FDRangeOfFreedom" },
                                                            { 1690304616, "FDThreeDMatingPoint" },
                                                            { 1691353192, "FDThreeDMatingAxes" },
                                                            { 1693450344, "FDTwoDDegreeOfFreedomAxis" },
                                                            { 1694498920, "SQPlanningLandmarkPointSequence" },
                                                            { 1695547496, "SQPlanningLandmarkLineSequence" },
                                                            { 1696596072, "SQPlanningLandmarkPlaneSequence" },
                                                            { 1697644648, "USPlanningLandmarkID" },
                                                            { 1698693224, "LOPlanningLandmarkDescription" },
                                                            { 1699020904, "SQPlanningLandmarkIdentificationCodeSequence" },
                                                            { 1699741800, "SQTwoDPointCoordinatesSequence" },
                                                            { 1700790376, "FDTwoDPointCoordinates" },
                                                            { 1703936104, "FDThreeDPointCoordinates" },
                                                            { 1704984680, "SQTwoDLineCoordinatesSequence" },
                                                            { 1706033256, "FDTwoDLineCoordinates" },
                                                            { 1708130408, "FDThreeDLineCoordinates" },
                                                            { 1709178984, "SQTwoDPlaneCoordinatesSequence" },
                                                            { 1710227560, "FDTwoDPlaneIntersection" },
                                                            { 1712324712, "FDThreeDPlaneOrigin" },
                                                            { 1713373288, "FDThreeDPlaneNormal" },
                                                            { 65648, "SQGraphicAnnotationSequence" },
                                                            { 131184, "CSGraphicLayer" },
                                                            { 196720, "CSBoundingBoxAnnotationUnits" },
                                                            { 262256, "CSAnchorPointAnnotationUnits" },
                                                            { 327792, "CSGraphicAnnotationUnits" },
                                                            { 393328, "STUnformattedTextValue" },
                                                            { 524400, "SQTextObjectSequence" },
                                                            { 589936, "SQGraphicObjectSequence" },
                                                            { 1048688, "FLBoundingBoxTopLeftHandCorner" },
                                                            { 1114224, "FLBoundingBoxBottomRightHandCorner" },
                                                            { 1179760, "CSBoundingBoxTextHorizontalJustification" },
                                                            { 1310832, "FLAnchorPoint" },
                                                            { 1376368, "CSAnchorPointVisibility" },
                                                            { 2097264, "USGraphicDimensions" },
                                                            { 2162800, "USNumberOfGraphicPoints" },
                                                            { 2228336, "FLGraphicData" },
                                                            { 2293872, "CSGraphicType" },
                                                            { 2359408, "CSGraphicFilled" },
                                                            { 4194416, "ISImageRotationRetired" },
                                                            { 4259952, "CSImageHorizontalFlip" },
                                                            { 4325488, "USImageRotation" },
                                                            { 5242992, "USDisplayedAreaTopLeftHandCornerTrial" },
                                                            { 5308528, "USDisplayedAreaBottomRightHandCornerTrial" },
                                                            { 5374064, "SLDisplayedAreaTopLeftHandCorner" },
                                                            { 5439600, "SLDisplayedAreaBottomRightHandCorner" },
                                                            { 5898352, "SQDisplayedAreaSelectionSequence" },
                                                            { 6291568, "SQGraphicLayerSequence" },
                                                            { 6422640, "ISGraphicLayerOrder" },
                                                            { 6684784, "USGraphicLayerRecommendedDisplayGrayscaleValue" },
                                                            { 6750320, "USGraphicLayerRecommendedDisplayRGBValue" },
                                                            { 6815856, "LOGraphicLayerDescription" },
                                                            { 8388720, "CSContentLabel" },
                                                            { 8454256, "LOContentDescription" },
                                                            { 8519792, "DAPresentationCreationDate" },
                                                            { 8585328, "TMPresentationCreationTime" },
                                                            { 8650864, "PNContentCreatorName" },
                                                            { 8781936, "SQContentCreatorIdentificationCodeSequence" },
                                                            { 8847472, "SQAlternateContentDescriptionSequence" },
                                                            { 16777328, "CSPresentationSizeMode" },
                                                            { 16842864, "DSPresentationPixelSpacing" },
                                                            { 16908400, "ISPresentationPixelAspectRatio" },
                                                            { 16973936, "FLPresentationPixelMagnificationRatio" },
                                                            { 34013296, "LOGraphicGroupLabel" },
                                                            { 34078832, "STGraphicGroupDescription" },
                                                            { 34144368, "SQCompoundGraphicSequence" },
                                                            { 36044912, "ULCompoundGraphicInstanceID" },
                                                            { 36110448, "LOFontName" },
                                                            { 36175984, "CSFontNameType" },
                                                            { 36241520, "LOCSSFontName" },
                                                            { 36700272, "FDRotationAngle" },
                                                            { 36765808, "SQTextStyleSequence" },
                                                            { 36831344, "SQLineStyleSequence" },
                                                            { 36896880, "SQFillStyleSequence" },
                                                            { 36962416, "SQGraphicGroupSequence" },
                                                            { 37814384, "USTextColorCIELabValue" },
                                                            { 37879920, "CSHorizontalAlignment" },
                                                            { 37945456, "CSVerticalAlignment" },
                                                            { 38010992, "CSShadowStyle" },
                                                            { 38076528, "FLShadowOffsetX" },
                                                            { 38142064, "FLShadowOffsetY" },
                                                            { 38207600, "USShadowColorCIELabValue" },
                                                            { 38273136, "CSUnderlined" },
                                                            { 38338672, "CSBold" },
                                                            { 38797424, "CSItalic" },
                                                            { 38862960, "USPatternOnColorCIELabValue" },
                                                            { 38928496, "USPatternOffColorCIELabValue" },
                                                            { 38994032, "FLLineThickness" },
                                                            { 39059568, "CSLineDashingStyle" },
                                                            { 39125104, "ULLinePattern" },
                                                            { 39190640, "OBFillPattern" },
                                                            { 39256176, "CSFillMode" },
                                                            { 39321712, "FLShadowOpacity" },
                                                            { 39911536, "FLGapLength" },
                                                            { 39977072, "FLDiameterOfVisibility" },
                                                            { 41091184, "FLRotationPoint" },
                                                            { 41156720, "CSTickAlignment" },
                                                            { 41418864, "CSShowTickLabel" },
                                                            { 41484400, "CSTickLabelAlignment" },
                                                            { 42074224, "CSCompoundGraphicUnits" },
                                                            { 42205296, "FLPatternOnOpacity" },
                                                            { 42270832, "FLPatternOffOpacity" },
                                                            { 42401904, "SQMajorTicksSequence" },
                                                            { 42467440, "FLTickPosition" },
                                                            { 42532976, "SHTickLabel" },
                                                            { 43253872, "CSCompoundGraphicType" },
                                                            { 43319408, "ULGraphicGroupID" },
                                                            { 50724976, "CSShapeType" },
                                                            { 50856048, "SQRegistrationSequence" },
                                                            { 50921584, "SQMatrixRegistrationSequence" },
                                                            { 50987120, "SQMatrixSequence" },
                                                            { 51052656, "FDFrameOfReferenceToDisplayedCoordinateSystemTransformationMatrix" },
                                                            { 51118192, "CSFrameOfReferenceTransformationMatrixType" },
                                                            { 51183728, "SQRegistrationTypeCodeSequence" },
                                                            { 51314800, "STFiducialDescription" },
                                                            { 51380336, "SHFiducialIdentifier" },
                                                            { 51445872, "SQFiducialIdentifierCodeSequence" },
                                                            { 51511408, "FDContourUncertaintyRadius" },
                                                            { 51642480, "SQUsedFiducialsSequence" },
                                                            { 51904624, "SQGraphicCoordinatesDataSequence" },
                                                            { 52035696, "UIFiducialUID" },
                                                            { 52166768, "SQFiducialSetSequence" },
                                                            { 52297840, "SQFiducialSequence" },
                                                            { 52363376, "SQFiducialsPropertyCategoryCodeSequence" },
                                                            { 67174512, "USGraphicLayerRecommendedDisplayCIELabValue" },
                                                            { 67240048, "SQBlendingSequence" },
                                                            { 67305584, "FLRelativeOpacity" },
                                                            { 67371120, "SQReferencedSpatialRegistrationSequence" },
                                                            { 67436656, "CSBlendingPosition" },
                                                            { 285278320, "UIPresentationDisplayCollectionUID" },
                                                            { 285343856, "UIPresentationSequenceCollectionUID" },
                                                            { 285409392, "USPresentationSequencePositionIndex" },
                                                            { 285474928, "SQRenderedImageReferenceSequence" },
                                                            { 302055536, "SQVolumetricPresentationStateInputSequence" },
                                                            { 302121072, "CSPresentationInputType" },
                                                            { 302186608, "USInputSequencePositionIndex" },
                                                            { 302252144, "CSCrop" },
                                                            { 302317680, "USCroppingSpecificationIndex" },
                                                            { 302383216, "CSCompositingMethod" },
                                                            { 302448752, "USVolumetricPresentationInputNumber" },
                                                            { 302514288, "CSImageVolumeGeometry" },
                                                            { 318832752, "SQVolumeCroppingSequence" },
                                                            { 318898288, "CSVolumeCroppingMethod" },
                                                            { 318963824, "FDBoundingBoxCrop" },
                                                            { 319029360, "SQObliqueCroppingPlaneSequence" },
                                                            { 319094896, "FDPlane" },
                                                            { 319160432, "FDPlaneNormal" },
                                                            { 319357040, "USCroppingSpecificationNumber" },
                                                            { 352387184, "CSMultiPlanarReconstructionStyle" },
                                                            { 352452720, "CSMPRThicknessType" },
                                                            { 352518256, "FDMPRSlabThickness" },
                                                            { 352649328, "FDMPRTopLeftHandCorner" },
                                                            { 352780400, "FDMPRViewWidthDirection" },
                                                            { 352845936, "FDMPRViewWidth" },
                                                            { 353108080, "ULNumberOfVolumetricCurvePoints" },
                                                            { 353173616, "ODVolumetricCurvePoints" },
                                                            { 353435760, "FDMPRViewHeightDirection" },
                                                            { 353501296, "FDMPRViewHeight" },
                                                            { 402718832, "SQPresentationStateClassificationComponentSequence" },
                                                            { 402784368, "CSComponentType" },
                                                            { 402849904, "SQComponentInputSequence" },
                                                            { 402915440, "USVolumetricPresentationInputIndex" },
                                                            { 402980976, "SQPresentationStateCompositorComponentSequence" },
                                                            { 403046512, "SQWeightingTransferFunctionSequence" },
                                                            { 403112048, "USWeightingLookupTableDescriptor" },
                                                            { 403177584, "OBWeightingLookupTableData" },
                                                            { 419496048, "SQVolumetricAnnotationSequence" },
                                                            { 419627120, "SQReferencedStructuredContextSequence" },
                                                            { 419692656, "UIReferencedContentItem" },
                                                            { 419758192, "SQVolumetricPresentationInputAnnotationSequence" },
                                                            { 419889264, "CSAnnotationClipping" },
                                                            { 436273264, "CSPresentationAnimationStyle" },
                                                            { 436404336, "FDRecommendedAnimationRate" },
                                                            { 436469872, "SQAnimationCurveSequence" },
                                                            { 436535408, "FDAnimationStepSize" },
                                                            { 131186, "SHHangingProtocolName" },
                                                            { 262258, "LOHangingProtocolDescription" },
                                                            { 393330, "CSHangingProtocolLevel" },
                                                            { 524402, "LOHangingProtocolCreator" },
                                                            { 655474, "DTHangingProtocolCreationDateTime" },
                                                            { 786546, "SQHangingProtocolDefinitionSequence" },
                                                            { 917618, "SQHangingProtocolUserIdentificationCodeSequence" },
                                                            { 1048690, "LOHangingProtocolUserGroupName" },
                                                            { 1179762, "SQSourceHangingProtocolSequence" },
                                                            { 1310834, "USNumberOfPriorsReferenced" },
                                                            { 2097266, "SQImageSetsSequence" },
                                                            { 2228338, "SQImageSetSelectorSequence" },
                                                            { 2359410, "CSImageSetSelectorUsageFlag" },
                                                            { 2490482, "ATSelectorAttribute" },
                                                            { 2621554, "USSelectorValueNumber" },
                                                            { 3145842, "SQTimeBasedImageSetsSequence" },
                                                            { 3276914, "USImageSetNumber" },
                                                            { 3407986, "CSImageSetSelectorCategory" },
                                                            { 3670130, "USRelativeTime" },
                                                            { 3801202, "CSRelativeTimeUnits" },
                                                            { 3932274, "SSAbstractPriorValue" },
                                                            { 4063346, "SQAbstractPriorCodeSequence" },
                                                            { 4194418, "LOImageSetLabel" },
                                                            { 5242994, "CSSelectorAttributeVR" },
                                                            { 5374066, "ATSelectorSequencePointer" },
                                                            { 5505138, "LOSelectorSequencePointerPrivateCreator" },
                                                            { 5636210, "LOSelectorAttributePrivateCreator" },
                                                            { 6160498, "AESelectorAEValue" },
                                                            { 6226034, "ASSelectorASValue" },
                                                            { 6291570, "ATSelectorATValue" },
                                                            { 6357106, "DASelectorDAValue" },
                                                            { 6422642, "CSSelectorCSValue" },
                                                            { 6488178, "DTSelectorDTValue" },
                                                            { 6553714, "ISSelectorISValue" },
                                                            { 6619250, "OBSelectorOBValue" },
                                                            { 6684786, "LOSelectorLOValue" },
                                                            { 6750322, "OFSelectorOFValue" },
                                                            { 6815858, "LTSelectorLTValue" },
                                                            { 6881394, "OWSelectorOWValue" },
                                                            { 6946930, "PNSelectorPNValue" },
                                                            { 7012466, "TMSelectorTMValue" },
                                                            { 7078002, "SHSelectorSHValue" },
                                                            { 7143538, "UNSelectorUNValue" },
                                                            { 7209074, "STSelectorSTValue" },
                                                            { 7274610, "UCSelectorUCValue" },
                                                            { 7340146, "UTSelectorUTValue" },
                                                            { 7405682, "URSelectorURValue" },
                                                            { 7471218, "DSSelectorDSValue" },
                                                            { 7536754, "ODSelectorODValue" },
                                                            { 7602290, "FDSelectorFDValue" },
                                                            { 7667826, "OLSelectorOLValue" },
                                                            { 7733362, "FLSelectorFLValue" },
                                                            { 7864434, "ULSelectorULValue" },
                                                            { 7995506, "USSelectorUSValue" },
                                                            { 8126578, "SLSelectorSLValue" },
                                                            { 8257650, "SSSelectorSSValue" },
                                                            { 8323186, "UISelectorUIValue" },
                                                            { 8388722, "SQSelectorCodeSequenceValue" },
                                                            { 16777330, "USNumberOfScreens" },
                                                            { 16908402, "SQNominalScreenDefinitionSequence" },
                                                            { 17039474, "USNumberOfVerticalPixels" },
                                                            { 17170546, "USNumberOfHorizontalPixels" },
                                                            { 17301618, "FDDisplayEnvironmentSpatialPosition" },
                                                            { 17432690, "USScreenMinimumGrayscaleBitDepth" },
                                                            { 17563762, "USScreenMinimumColorBitDepth" },
                                                            { 17694834, "USApplicationMaximumRepaintTime" },
                                                            { 33554546, "SQDisplaySetsSequence" },
                                                            { 33685618, "USDisplaySetNumber" },
                                                            { 33751154, "LODisplaySetLabel" },
                                                            { 33816690, "USDisplaySetPresentationGroup" },
                                                            { 33947762, "LODisplaySetPresentationGroupDescription" },
                                                            { 34078834, "CSPartialDataDisplayHandling" },
                                                            { 34603122, "SQSynchronizedScrollingSequence" },
                                                            { 34734194, "USDisplaySetScrollingGroup" },
                                                            { 34865266, "SQNavigationIndicatorSequence" },
                                                            { 34996338, "USNavigationDisplaySet" },
                                                            { 35127410, "USReferenceDisplaySets" },
                                                            { 50331762, "SQImageBoxesSequence" },
                                                            { 50462834, "USImageBoxNumber" },
                                                            { 50593906, "CSImageBoxLayoutType" },
                                                            { 50724978, "USImageBoxTileHorizontalDimension" },
                                                            { 50856050, "USImageBoxTileVerticalDimension" },
                                                            { 51380338, "CSImageBoxScrollDirection" },
                                                            { 51511410, "CSImageBoxSmallScrollType" },
                                                            { 51642482, "USImageBoxSmallScrollAmount" },
                                                            { 51773554, "CSImageBoxLargeScrollType" },
                                                            { 51904626, "USImageBoxLargeScrollAmount" },
                                                            { 52428914, "USImageBoxOverlapPriority" },
                                                            { 53477490, "FDCineRelativeToRealTime" },
                                                            { 67108978, "SQFilterOperationsSequence" },
                                                            { 67240050, "CSFilterByCategory" },
                                                            { 67371122, "CSFilterByAttributePresence" },
                                                            { 67502194, "CSFilterByOperator" },
                                                            { 69206130, "USStructuredDisplayBackgroundCIELabValue" },
                                                            { 69271666, "USEmptyImageBoxCIELabValue" },
                                                            { 69337202, "SQStructuredDisplayImageBoxSequence" },
                                                            { 69468274, "SQStructuredDisplayTextBoxSequence" },
                                                            { 69664882, "SQReferencedFirstFrameSequence" },
                                                            { 70254706, "SQImageBoxSynchronizationSequence" },
                                                            { 70385778, "USSynchronizedImageBoxList" },
                                                            { 70516850, "CSTypeOfSynchronization" },
                                                            { 83886194, "CSBlendingOperationType" },
                                                            { 84934770, "CSReformattingOperationType" },
                                                            { 85065842, "FDReformattingThickness" },
                                                            { 85196914, "FDReformattingInterval" },
                                                            { 85327986, "CSReformattingOperationInitialViewDirection" },
                                                            { 85983346, "CSThreeDRenderingType" },
                                                            { 100663410, "SQSortingOperationsSequence" },
                                                            { 100794482, "CSSortByCategory" },
                                                            { 100925554, "CSSortingDirection" },
                                                            { 117440626, "CSDisplaySetPatientOrientation" },
                                                            { 117571698, "CSVOIType" },
                                                            { 117702770, "CSPseudoColorType" },
                                                            { 117768306, "SQPseudoColorPaletteInstanceReferenceSequence" },
                                                            { 117833842, "CSShowGrayscaleInverted" },
                                                            { 118489202, "CSShowImageTrueSizeFlag" },
                                                            { 118620274, "CSShowGraphicAnnotationFlag" },
                                                            { 118751346, "CSShowPatientDemographicsFlag" },
                                                            { 118882418, "CSShowAcquisitionTechniquesFlag" },
                                                            { 118947954, "CSDisplaySetHorizontalJustification" },
                                                            { 119013490, "CSDisplaySetVerticalJustification" },
                                                            { 18874484, "FDContinuationStartMeterset" },
                                                            { 18940020, "FDContinuationEndMeterset" },
                                                            { 268435572, "CSProcedureStepState" },
                                                            { 268566644, "SQProcedureStepProgressInformationSequence" },
                                                            { 268697716, "DSProcedureStepProgress" },
                                                            { 268828788, "STProcedureStepProgressDescription" },
                                                            { 268959860, "SQProcedureStepCommunicationsURISequence" },
                                                            { 269090932, "URContactURI" },
                                                            { 269222004, "LOContactDisplayName" },
                                                            { 269353076, "SQProcedureStepDiscontinuationReasonCodeSequence" },
                                                            { 270532724, "SQBeamTaskSequence" },
                                                            { 270663796, "CSBeamTaskType" },
                                                            { 270794868, "ISBeamOrderIndexTrial" },
                                                            { 270860404, "CSAutosequenceFlag" },
                                                            { 270925940, "FDTableTopVerticalAdjustedPosition" },
                                                            { 270991476, "FDTableTopLongitudinalAdjustedPosition" },
                                                            { 271057012, "FDTableTopLateralAdjustedPosition" },
                                                            { 271188084, "FDPatientSupportAdjustedAngle" },
                                                            { 271253620, "FDTableTopEccentricAdjustedAngle" },
                                                            { 271319156, "FDTableTopPitchAdjustedAngle" },
                                                            { 271384692, "FDTableTopRollAdjustedAngle" },
                                                            { 271581300, "SQDeliveryVerificationImageSequence" },
                                                            { 271712372, "CSVerificationImageTiming" },
                                                            { 271843444, "CSDoubleExposureFlag" },
                                                            { 271974516, "CSDoubleExposureOrdering" },
                                                            { 272105588, "DSDoubleExposureMetersetTrial" },
                                                            { 272236660, "DSDoubleExposureFieldDeltaTrial" },
                                                            { 272629876, "SQRelatedReferenceRTImageSequence" },
                                                            { 272760948, "SQGeneralMachineVerificationSequence" },
                                                            { 272892020, "SQConventionalMachineVerificationSequence" },
                                                            { 273023092, "SQIonMachineVerificationSequence" },
                                                            { 273154164, "SQFailedAttributesSequence" },
                                                            { 273285236, "SQOverriddenAttributesSequence" },
                                                            { 273416308, "SQConventionalControlPointVerificationSequence" },
                                                            { 273547380, "SQIonControlPointVerificationSequence" },
                                                            { 273678452, "SQAttributeOccurrenceSequence" },
                                                            { 273809524, "ATAttributeOccurrencePointer" },
                                                            { 273940596, "ULAttributeItemSelector" },
                                                            { 274071668, "LOAttributeOccurrencePrivateCreator" },
                                                            { 274137204, "ISSelectorSequencePointerItems" },
                                                            { 301990004, "CSScheduledProcedureStepPriority" },
                                                            { 302121076, "LOWorklistLabel" },
                                                            { 302252148, "LOProcedureStepLabel" },
                                                            { 303038580, "SQScheduledProcessingParametersSequence" },
                                                            { 303169652, "SQPerformedProcessingParametersSequence" },
                                                            { 303431796, "SQUnifiedProcedureStepPerformedProcedureSequence" },
                                                            { 304087156, "SQRelatedProcedureStepSequence" },
                                                            { 304218228, "LOProcedureStepRelationshipType" },
                                                            { 304349300, "SQReplacedProcedureStepSequence" },
                                                            { 305135732, "LODeletionLock" },
                                                            { 305397876, "AEReceivingAE" },
                                                            { 305528948, "AERequestingAE" },
                                                            { 305660020, "LTReasonForCancellation" },
                                                            { 306315380, "CSSCPStatus" },
                                                            { 306446452, "CSSubscriptionListStatus" },
                                                            { 306577524, "CSUnifiedProcedureStepListStatus" },
                                                            { 321126516, "ULBeamOrderIndex" },
                                                            { 322437236, "FDDoubleExposureMeterset" },
                                                            { 322568308, "FDDoubleExposureFieldDelta" },
                                                            { 335609972, "SQBrachyTaskSequence" },
                                                            { 335675508, "DSContinuationStartTotalReferenceAirKerma" },
                                                            { 335741044, "DSContinuationEndTotalReferenceAirKerma" },
                                                            { 335806580, "ISContinuationPulseNumber" },
                                                            { 335872116, "SQChannelDeliveryOrderSequence" },
                                                            { 335937652, "ISReferencedChannelNumber" },
                                                            { 336003188, "DSStartCumulativeTimeWeight" },
                                                            { 336068724, "DSEndCumulativeTimeWeight" },
                                                            { 336134260, "SQOmittedChannelSequence" },
                                                            { 336199796, "CSReasonForChannelOmission" },
                                                            { 336265332, "LOReasonForChannelOmissionDescription" },
                                                            { 336330868, "ISChannelDeliveryOrderIndex" },
                                                            { 336396404, "SQChannelDeliveryContinuationSequence" },
                                                            { 336461940, "SQOmittedApplicationSetupSequence" },
                                                            { 65654, "LOImplantAssemblyTemplateName" },
                                                            { 196726, "LOImplantAssemblyTemplateIssuer" },
                                                            { 393334, "LOImplantAssemblyTemplateVersion" },
                                                            { 524406, "SQReplacedImplantAssemblyTemplateSequence" },
                                                            { 655478, "CSImplantAssemblyTemplateType" },
                                                            { 786550, "SQOriginalImplantAssemblyTemplateSequence" },
                                                            { 917622, "SQDerivationImplantAssemblyTemplateSequence" },
                                                            { 1048694, "SQImplantAssemblyTemplateTargetAnatomySequence" },
                                                            { 2097270, "SQProcedureTypeCodeSequence" },
                                                            { 3145846, "LOSurgicalTechnique" },
                                                            { 3276918, "SQComponentTypesSequence" },
                                                            { 3407990, "CSComponentTypeCodeSequence" },
                                                            { 3539062, "CSExclusiveComponentType" },
                                                            { 3670134, "CSMandatoryComponentType" },
                                                            { 4194422, "SQComponentSequence" },
                                                            { 5570678, "USComponentID" },
                                                            { 6291574, "SQComponentAssemblySequence" },
                                                            { 7340150, "USComponent1ReferencedID" },
                                                            { 8388726, "USComponent1ReferencedMatingFeatureSetID" },
                                                            { 9437302, "USComponent1ReferencedMatingFeatureID" },
                                                            { 10485878, "USComponent2ReferencedID" },
                                                            { 11534454, "USComponent2ReferencedMatingFeatureSetID" },
                                                            { 12583030, "USComponent2ReferencedMatingFeatureID" },
                                                            { 65656, "LOImplantTemplateGroupName" },
                                                            { 1048696, "STImplantTemplateGroupDescription" },
                                                            { 2097272, "LOImplantTemplateGroupIssuer" },
                                                            { 2359416, "LOImplantTemplateGroupVersion" },
                                                            { 2490488, "SQReplacedImplantTemplateGroupSequence" },
                                                            { 2621560, "SQImplantTemplateGroupTargetAnatomySequence" },
                                                            { 2752632, "SQImplantTemplateGroupMembersSequence" },
                                                            { 3014776, "USImplantTemplateGroupMemberID" },
                                                            { 5243000, "FDThreeDImplantTemplateGroupMemberMatchingPoint" },
                                                            { 6291576, "FDThreeDImplantTemplateGroupMemberMatchingAxes" },
                                                            { 7340152, "SQImplantTemplateGroupMemberMatching2DCoordinatesSequence" },
                                                            { 9437304, "FDTwoDImplantTemplateGroupMemberMatchingPoint" },
                                                            { 10485880, "FDTwoDImplantTemplateGroupMemberMatchingAxes" },
                                                            { 11534456, "SQImplantTemplateGroupVariationDimensionSequence" },
                                                            { 11665528, "LOImplantTemplateGroupVariationDimensionName" },
                                                            { 11796600, "SQImplantTemplateGroupVariationDimensionRankSequence" },
                                                            { 11927672, "USReferencedImplantTemplateGroupMemberID" },
                                                            { 12058744, "USImplantTemplateGroupVariationDimensionRank" },
                                                            { 65664, "SQSurfaceScanAcquisitionTypeCodeSequence" },
                                                            { 131200, "SQSurfaceScanModeCodeSequence" },
                                                            { 196736, "SQRegistrationMethodCodeSequence" },
                                                            { 262272, "FDShotDurationTime" },
                                                            { 327808, "FDShotOffsetTime" },
                                                            { 393344, "USSurfacePointPresentationValueData" },
                                                            { 458880, "USSurfacePointColorCIELabValueData" },
                                                            { 524416, "SQUVMappingSequence" },
                                                            { 589952, "SHTextureLabel" },
                                                            { 1048704, "OFUValueData" },
                                                            { 1114240, "OFVValueData" },
                                                            { 1179776, "SQReferencedTextureSequence" },
                                                            { 1245312, "SQReferencedSurfaceDataSequence" },
                                                            { 65666, "CSAssessmentSummary" },
                                                            { 196738, "UTAssessmentSummaryDescription" },
                                                            { 262274, "SQAssessedSOPInstanceSequence" },
                                                            { 327810, "SQReferencedComparisonSOPInstanceSequence" },
                                                            { 393346, "ULNumberOfAssessmentObservations" },
                                                            { 458882, "SQAssessmentObservationsSequence" },
                                                            { 524418, "CSObservationSignificance" },
                                                            { 655490, "UTObservationDescription" },
                                                            { 786562, "SQStructuredContraintObservationSequence" },
                                                            { 1048706, "SQAssessedAttributeValueSequence" },
                                                            { 1441922, "LOAssessmentSetID" },
                                                            { 1507458, "SQAssessmentRequesterSequence" },
                                                            { 1572994, "LOSelectorAttributeName" },
                                                            { 1638530, "LOSelectorAttributeKeyword" },
                                                            { 2162818, "SQAssessmentTypeCodeSequence" },
                                                            { 2228354, "SQObservationBasisCodeSequence" },
                                                            { 2293890, "LOAssessmentLabel" },
                                                            { 3276930, "CSConstraintType" },
                                                            { 3342466, "UTSpecificationSelectionGuidance" },
                                                            { 3408002, "SQConstraintValueSequence" },
                                                            { 3473538, "SQRecommendedDefaultValueSequence" },
                                                            { 3539074, "CSConstraintViolationSignificance" },
                                                            { 3604610, "UTConstraintViolationCondition" },
                                                            { 3670146, "CSModifiableConstraintFlag" },
                                                            { 19923080, "SHStorageMediaFileSetID" },
                                                            { 20971656, "UIStorageMediaFileSetUID" },
                                                            { 33554568, "SQIconImageSequence" },
                                                            { 151257224, "LOTopicTitle" },
                                                            { 151388296, "STTopicSubject" },
                                                            { 152043656, "LOTopicAuthor" },
                                                            { 152174728, "LOTopicKeywords" },
                                                            { 68157696, "CSSOPInstanceStatus" },
                                                            { 69206272, "DTSOPAuthorizationDateTime" },
                                                            { 69468416, "LTSOPAuthorizationComment" },
                                                            { 69599488, "LOAuthorizationEquipmentCertificationNumber" },
                                                            { 328704, "USMACIDNumber" },
                                                            { 1049600, "UIMACCalculationTransferSyntaxUID" },
                                                            { 1377280, "CSMACAlgorithm" },
                                                            { 2098176, "ATDataElementsSigned" },
                                                            { 16778240, "UIDigitalSignatureUID" },
                                                            { 17105920, "DTDigitalSignatureDateTime" },
                                                            { 17826816, "CSCertificateType" },
                                                            { 18154496, "OBCertificateOfSigner" },
                                                            { 18875392, "OBSignature" },
                                                            { 50660352, "CSCertifiedTimestampType" },
                                                            { 51381248, "OBCertifiedTimestamp" },
                                                            { 67175424, "SQDigitalSignaturePurposeCodeSequence" },
                                                            { 67240960, "SQReferencedDigitalSignatureSequence" },
                                                            { 67306496, "SQReferencedSOPInstanceMACSequence" },
                                                            { 67372032, "OBMAC" },
                                                            { 83887104, "SQEncryptedAttributesSequence" },
                                                            { 84935680, "UIEncryptedContentTransferSyntaxUID" },
                                                            { 85984256, "OBEncryptedContent" },
                                                            { 89129984, "SQModifiedAttributesSequence" },
                                                            { 90244096, "SQOriginalAttributesSequence" },
                                                            { 90309632, "DTAttributeModificationDateTime" },
                                                            { 90375168, "LOModifyingSystem" },
                                                            { 90440704, "LOSourceOfPreviousValues" },
                                                            { 90506240, "CSReasonForTheAttributeModification" },
                                                            { 4096, "USEscapeTriplet" },
                                                            { 69632, "USRunLengthTriplet" },
                                                            { 135168, "USHuffmanTableSize" },
                                                            { 200704, "USHuffmanTableTriplet" },
                                                            { 266240, "USShiftTableSize" },
                                                            { 331776, "USShiftTableTriplet" },
                                                            { 4112, "USZonalMap" },
                                                            { 1056768, "ISNumberOfCopies" },
                                                            { 1974272, "SQPrinterConfigurationSequence" },
                                                            { 2105344, "CSPrintPriority" },
                                                            { 3153920, "CSMediumType" },
                                                            { 4202496, "CSFilmDestination" },
                                                            { 5251072, "LOFilmSessionLabel" },
                                                            { 6299648, "ISMemoryAllocation" },
                                                            { 6365184, "ISMaximumMemoryAllocation" },
                                                            { 6430720, "CSColorImagePrintingFlag" },
                                                            { 6496256, "CSCollationFlag" },
                                                            { 6627328, "CSAnnotationFlag" },
                                                            { 6758400, "CSImageOverlayFlag" },
                                                            { 6889472, "CSPresentationLUTFlag" },
                                                            { 6955008, "CSImageBoxPresentationLUTFlag" },
                                                            { 10493952, "USMemoryBitDepth" },
                                                            { 10559488, "USPrintingBitDepth" },
                                                            { 10625024, "SQMediaInstalledSequence" },
                                                            { 10756096, "SQOtherMediaAvailableSequence" },
                                                            { 11018240, "SQSupportedImageDisplayFormatsSequence" },
                                                            { 83894272, "SQReferencedFilmBoxSequence" },
                                                            { 84942848, "SQReferencedStoredPrintSequence" },
                                                            { 1056784, "STImageDisplayFormat" },
                                                            { 3153936, "CSAnnotationDisplayFormatID" },
                                                            { 4202512, "CSFilmOrientation" },
                                                            { 5251088, "CSFilmSizeID" },
                                                            { 5382160, "CSPrinterResolutionID" },
                                                            { 5513232, "CSDefaultPrinterResolutionID" },
                                                            { 6299664, "CSMagnificationType" },
                                                            { 8396816, "CSSmoothingType" },
                                                            { 10887184, "CSDefaultMagnificationType" },
                                                            { 10952720, "CSOtherMagnificationTypesAvailable" },
                                                            { 11018256, "CSDefaultSmoothingType" },
                                                            { 11083792, "CSOtherSmoothingTypesAvailable" },
                                                            { 16785424, "CSBorderDensity" },
                                                            { 17834000, "CSEmptyImageDensity" },
                                                            { 18882576, "USMinDensity" },
                                                            { 19931152, "USMaxDensity" },
                                                            { 20979728, "CSTrim" },
                                                            { 22028304, "STConfigurationInformation" },
                                                            { 22159376, "LTConfigurationInformationDescription" },
                                                            { 22290448, "ISMaximumCollatedFilms" },
                                                            { 22945808, "USIllumination" },
                                                            { 23076880, "USReflectedAmbientLight" },
                                                            { 58073104, "DSPrinterPixelSpacing" },
                                                            { 83894288, "SQReferencedFilmSessionSequence" },
                                                            { 84942864, "SQReferencedImageBoxSequence" },
                                                            { 85991440, "SQReferencedBasicAnnotationBoxSequence" },
                                                            { 1056800, "USImageBoxPosition" },
                                                            { 2105376, "CSPolarity" },
                                                            { 3153952, "DSRequestedImageSize" },
                                                            { 4202528, "CSRequestedDecimateCropBehavior" },
                                                            { 5251104, "CSRequestedResolutionID" },
                                                            { 10493984, "CSRequestedImageSizeFlag" },
                                                            { 10625056, "CSDecimateCropResult" },
                                                            { 17834016, "SQBasicGrayscaleImageSequence" },
                                                            { 17899552, "SQBasicColorImageSequence" },
                                                            { 19931168, "SQReferencedImageOverlayBoxSequence" },
                                                            { 20979744, "SQReferencedVOILUTBoxSequence" },
                                                            { 1056816, "USAnnotationPosition" },
                                                            { 2105392, "LOTextString" },
                                                            { 1056832, "SQReferencedOverlayPlaneSequence" },
                                                            { 1122368, "USReferencedOverlayPlaneGroups" },
                                                            { 2105408, "SQOverlayPixelDataSequence" },
                                                            { 6299712, "CSOverlayMagnificationType" },
                                                            { 7348288, "CSOverlaySmoothingType" },
                                                            { 7479360, "CSOverlayOrImageMagnification" },
                                                            { 7610432, "USMagnifyToNumberOfColumns" },
                                                            { 8396864, "CSOverlayForegroundDensity" },
                                                            { 8527936, "CSOverlayBackgroundDensity" },
                                                            { 9445440, "CSOverlayMode" },
                                                            { 16785472, "CSThresholdDensity" },
                                                            { 83894336, "SQReferencedImageBoxSequenceRetired" },
                                                            { 1056848, "SQPresentationLUTSequence" },
                                                            { 2105424, "CSPresentationLUTShape" },
                                                            { 83894352, "SQReferencedPresentationLUTSequence" },
                                                            { 1057024, "SHPrintJobID" },
                                                            { 2105600, "CSExecutionStatus" },
                                                            { 3154176, "CSExecutionStatusInfo" },
                                                            { 4202752, "DACreationDate" },
                                                            { 5251328, "TMCreationTime" },
                                                            { 7348480, "AEOriginator" },
                                                            { 20979968, "AEDestinationAE" },
                                                            { 23077120, "SHOwnerID" },
                                                            { 24125696, "ISNumberOfFilms" },
                                                            { 83894528, "SQReferencedPrintJobSequencePullStoredPrint" },
                                                            { 1057040, "CSPrinterStatus" },
                                                            { 2105616, "CSPrinterStatusInfo" },
                                                            { 3154192, "LOPrinterName" },
                                                            { 10035472, "SHPrintQueueID" },
                                                            { 1057056, "CSQueueStatus" },
                                                            { 5251360, "SQPrintJobDescriptionSequence" },
                                                            { 7348512, "SQReferencedPrintJobSequence" },
                                                            { 1057072, "SQPrintManagementCapabilitiesSequence" },
                                                            { 1384752, "SQPrinterCharacteristicsSequence" },
                                                            { 3154224, "SQFilmBoxContentSequence" },
                                                            { 4202800, "SQImageBoxContentSequence" },
                                                            { 5251376, "SQAnnotationContentSequence" },
                                                            { 6299952, "SQImageOverlayBoxContentSequence" },
                                                            { 8397104, "SQPresentationLUTContentSequence" },
                                                            { 10494256, "SQProposedStudySequence" },
                                                            { 12591408, "SQOriginalImageSequence" },
                                                            { 74240, "CSLabelUsingInformationExtractedFromInstances" },
                                                            { 139776, "UTLabelText" },
                                                            { 205312, "CSLabelStyleSelection" },
                                                            { 270848, "LTMediaDisposition" },
                                                            { 336384, "LTBarcodeValue" },
                                                            { 401920, "CSBarcodeSymbology" },
                                                            { 467456, "CSAllowMediaSplitting" },
                                                            { 532992, "CSIncludeNonDICOMObjects" },
                                                            { 598528, "CSIncludeDisplayApplication" },
                                                            { 664064, "CSPreserveCompositeInstancesAfterMediaCreation" },
                                                            { 729600, "USTotalNumberOfPiecesOfMediaCreated" },
                                                            { 795136, "LORequestedMediaApplicationProfile" },
                                                            { 860672, "SQReferencedStorageMediaSequence" },
                                                            { 926208, "ATFailureAttributes" },
                                                            { 991744, "CSAllowLossyCompression" },
                                                            { 2105856, "CSRequestPriority" },
                                                            { 143362, "SHRTImageLabel" },
                                                            { 208898, "LORTImageName" },
                                                            { 274434, "STRTImageDescription" },
                                                            { 667650, "CSReportedValuesOrigin" },
                                                            { 798722, "CSRTImagePlane" },
                                                            { 864258, "DSXRayImageReceptorTranslation" },
                                                            { 929794, "DSXRayImageReceptorAngle" },
                                                            { 1060866, "DSRTImageOrientation" },
                                                            { 1126402, "DSImagePlanePixelSpacing" },
                                                            { 1191938, "DSRTImagePosition" },
                                                            { 2109442, "SHRadiationMachineName" },
                                                            { 2240514, "DSRadiationMachineSAD" },
                                                            { 2371586, "DSRadiationMachineSSD" },
                                                            { 2502658, "DSRTImageSID" },
                                                            { 2633730, "DSSourceToReferenceObjectDistance" },
                                                            { 2699266, "ISFractionNumber" },
                                                            { 3158018, "SQExposureSequence" },
                                                            { 3289090, "DSMetersetExposure" },
                                                            { 3420162, "DSDiaphragmPosition" },
                                                            { 4206594, "SQFluenceMapSequence" },
                                                            { 4272130, "CSFluenceDataSource" },
                                                            { 4337666, "DSFluenceDataScale" },
                                                            { 5255170, "SQPrimaryFluenceModeSequence" },
                                                            { 5320706, "CSFluenceMode" },
                                                            { 5386242, "SHFluenceModeID" },
                                                            { 77828, "CSDVHType" },
                                                            { 143364, "CSDoseUnits" },
                                                            { 274436, "CSDoseType" },
                                                            { 339972, "CSSpatialTransformOfDose" },
                                                            { 405508, "LODoseComment" },
                                                            { 536580, "DSNormalizationPoint" },
                                                            { 667652, "CSDoseSummationType" },
                                                            { 798724, "DSGridFrameOffsetVector" },
                                                            { 929796, "DSDoseGridScaling" },
                                                            { 1060868, "SQRTDoseROISequence" },
                                                            { 1191940, "DSDoseValue" },
                                                            { 1323012, "CSTissueHeterogeneityCorrection" },
                                                            { 4206596, "DSDVHNormalizationPoint" },
                                                            { 4337668, "DSDVHNormalizationDoseValue" },
                                                            { 5255172, "SQDVHSequence" },
                                                            { 5386244, "DSDVHDoseScaling" },
                                                            { 5517316, "CSDVHVolumeUnits" },
                                                            { 5648388, "ISDVHNumberOfBins" },
                                                            { 5779460, "DSDVHData" },
                                                            { 6303748, "SQDVHReferencedROISequence" },
                                                            { 6434820, "CSDVHROIContributionType" },
                                                            { 7352324, "DSDVHMinimumDose" },
                                                            { 7483396, "DSDVHMaximumDose" },
                                                            { 7614468, "DSDVHMeanDose" },
                                                            { 143366, "SHStructureSetLabel" },
                                                            { 274438, "LOStructureSetName" },
                                                            { 405510, "STStructureSetDescription" },
                                                            { 536582, "DAStructureSetDate" },
                                                            { 602118, "TMStructureSetTime" },
                                                            { 1060870, "SQReferencedFrameOfReferenceSequence" },
                                                            { 1191942, "SQRTReferencedStudySequence" },
                                                            { 1323014, "SQRTReferencedSeriesSequence" },
                                                            { 1454086, "SQContourImageSequence" },
                                                            { 1585158, "SQPredecessorStructureSetSequence" },
                                                            { 2109446, "SQStructureSetROISequence" },
                                                            { 2240518, "ISROINumber" },
                                                            { 2371590, "UIReferencedFrameOfReferenceUID" },
                                                            { 2502662, "LOROIName" },
                                                            { 2633734, "STROIDescription" },
                                                            { 2764806, "ISROIDisplayColor" },
                                                            { 2895878, "DSROIVolume" },
                                                            { 3158022, "SQRTRelatedROISequence" },
                                                            { 3354630, "CSRTROIRelationship" },
                                                            { 3551238, "CSROIGenerationAlgorithm" },
                                                            { 3682310, "LOROIGenerationDescription" },
                                                            { 3747846, "SQROIContourSequence" },
                                                            { 4206598, "SQContourSequence" },
                                                            { 4337670, "CSContourGeometricType" },
                                                            { 4468742, "DSContourSlabThickness" },
                                                            { 4534278, "DSContourOffsetVector" },
                                                            { 4599814, "ISNumberOfContourPoints" },
                                                            { 4730886, "ISContourNumber" },
                                                            { 4796422, "ISAttachedContours" },
                                                            { 5255174, "DSContourData" },
                                                            { 8400902, "SQRTROIObservationsSequence" },
                                                            { 8531974, "ISObservationNumber" },
                                                            { 8663046, "ISReferencedROINumber" },
                                                            { 8728582, "SHROIObservationLabel" },
                                                            { 8794118, "SQRTROIIdentificationCodeSequence" },
                                                            { 8925190, "STROIObservationDescription" },
                                                            { 10498054, "SQRelatedRTROIObservationsSequence" },
                                                            { 10760198, "CSRTROIInterpretedType" },
                                                            { 10891270, "PNROIInterpreter" },
                                                            { 11546630, "SQROIPhysicalPropertiesSequence" },
                                                            { 11677702, "CSROIPhysicalProperty" },
                                                            { 11808774, "DSROIPhysicalPropertyValue" },
                                                            { 11939846, "SQROIElementalCompositionSequence" },
                                                            { 12005382, "USROIElementalCompositionAtomicNumber" },
                                                            { 12070918, "FLROIElementalCompositionAtomicMassFraction" },
                                                            { 12136454, "SQAdditionalRTROIIdentificationCodeSequence" },
                                                            { 12595206, "SQFrameOfReferenceRelationshipSequence" },
                                                            { 12726278, "UIRelatedFrameOfReferenceUID" },
                                                            { 12857350, "CSFrameOfReferenceTransformationType" },
                                                            { 12988422, "DSFrameOfReferenceTransformationMatrix" },
                                                            { 13119494, "LOFrameOfReferenceTransformationComment" },
                                                            { 1060872, "SQMeasuredDoseReferenceSequence" },
                                                            { 1191944, "STMeasuredDoseDescription" },
                                                            { 1323016, "CSMeasuredDoseType" },
                                                            { 1454088, "DSMeasuredDoseValue" },
                                                            { 2109448, "SQTreatmentSessionBeamSequence" },
                                                            { 2174984, "SQTreatmentSessionIonBeamSequence" },
                                                            { 2240520, "ISCurrentFractionNumber" },
                                                            { 2371592, "DATreatmentControlPointDate" },
                                                            { 2437128, "TMTreatmentControlPointTime" },
                                                            { 2764808, "CSTreatmentTerminationStatus" },
                                                            { 2830344, "SHTreatmentTerminationCode" },
                                                            { 2895880, "CSTreatmentVerificationStatus" },
                                                            { 3158024, "SQReferencedTreatmentRecordSequence" },
                                                            { 3289096, "DSSpecifiedPrimaryMeterset" },
                                                            { 3354632, "DSSpecifiedSecondaryMeterset" },
                                                            { 3551240, "DSDeliveredPrimaryMeterset" },
                                                            { 3616776, "DSDeliveredSecondaryMeterset" },
                                                            { 3813384, "DSSpecifiedTreatmentTime" },
                                                            { 3878920, "DSDeliveredTreatmentTime" },
                                                            { 4206600, "SQControlPointDeliverySequence" },
                                                            { 4272136, "SQIonControlPointDeliverySequence" },
                                                            { 4337672, "DSSpecifiedMeterset" },
                                                            { 4468744, "DSDeliveredMeterset" },
                                                            { 4534280, "FLMetersetRateSet" },
                                                            { 4599816, "FLMetersetRateDelivered" },
                                                            { 4665352, "FLScanSpotMetersetsDelivered" },
                                                            { 4730888, "DSDoseRateDelivered" },
                                                            { 5255176, "SQTreatmentSummaryCalculatedDoseReferenceSequence" },
                                                            { 5386248, "DSCumulativeDoseToDoseReference" },
                                                            { 5517320, "DAFirstTreatmentDate" },
                                                            { 5648392, "DAMostRecentTreatmentDate" },
                                                            { 5910536, "ISNumberOfFractionsDelivered" },
                                                            { 6303752, "SQOverrideSequence" },
                                                            { 6369288, "ATParameterSequencePointer" },
                                                            { 6434824, "ATOverrideParameterPointer" },
                                                            { 6500360, "ISParameterItemIndex" },
                                                            { 6565896, "ISMeasuredDoseReferenceNumber" },
                                                            { 6631432, "ATParameterPointer" },
                                                            { 6696968, "STOverrideReason" },
                                                            { 6762504, "USParameterValueNumber" },
                                                            { 6828040, "SQCorrectedParameterSequence" },
                                                            { 6959112, "FLCorrectionValue" },
                                                            { 7352328, "SQCalculatedDoseReferenceSequence" },
                                                            { 7483400, "ISCalculatedDoseReferenceNumber" },
                                                            { 7614472, "STCalculatedDoseReferenceDescription" },
                                                            { 7745544, "DSCalculatedDoseReferenceDoseValue" },
                                                            { 7876616, "DSStartMeterset" },
                                                            { 8007688, "DSEndMeterset" },
                                                            { 8400904, "SQReferencedMeasuredDoseReferenceSequence" },
                                                            { 8531976, "ISReferencedMeasuredDoseReferenceNumber" },
                                                            { 9449480, "SQReferencedCalculatedDoseReferenceSequence" },
                                                            { 9580552, "ISReferencedCalculatedDoseReferenceNumber" },
                                                            { 10498056, "SQBeamLimitingDeviceLeafPairsSequence" },
                                                            { 11546632, "SQRecordedWedgeSequence" },
                                                            { 12595208, "SQRecordedCompensatorSequence" },
                                                            { 13643784, "SQRecordedBlockSequence" },
                                                            { 14692360, "SQTreatmentSummaryMeasuredDoseReferenceSequence" },
                                                            { 15740936, "SQRecordedSnoutSequence" },
                                                            { 15872008, "SQRecordedRangeShifterSequence" },
                                                            { 16003080, "SQRecordedLateralSpreadingDeviceSequence" },
                                                            { 16134152, "SQRecordedRangeModulatorSequence" },
                                                            { 16789512, "SQRecordedSourceSequence" },
                                                            { 17117192, "LOSourceSerialNumber" },
                                                            { 17838088, "SQTreatmentSessionApplicationSetupSequence" },
                                                            { 18231304, "CSApplicationSetupCheck" },
                                                            { 18886664, "SQRecordedBrachyAccessoryDeviceSequence" },
                                                            { 19017736, "ISReferencedBrachyAccessoryDeviceNumber" },
                                                            { 19935240, "SQRecordedChannelSequence" },
                                                            { 20066312, "DSSpecifiedChannelTotalTime" },
                                                            { 20197384, "DSDeliveredChannelTotalTime" },
                                                            { 20328456, "ISSpecifiedNumberOfPulses" },
                                                            { 20459528, "ISDeliveredNumberOfPulses" },
                                                            { 20590600, "DSSpecifiedPulseRepetitionInterval" },
                                                            { 20721672, "DSDeliveredPulseRepetitionInterval" },
                                                            { 20983816, "SQRecordedSourceApplicatorSequence" },
                                                            { 21114888, "ISReferencedSourceApplicatorNumber" },
                                                            { 22032392, "SQRecordedChannelShieldSequence" },
                                                            { 22163464, "ISReferencedChannelShieldNumber" },
                                                            { 23080968, "SQBrachyControlPointDeliveredSequence" },
                                                            { 23212040, "DASafePositionExitDate" },
                                                            { 23343112, "TMSafePositionExitTime" },
                                                            { 23474184, "DASafePositionReturnDate" },
                                                            { 23605256, "TMSafePositionReturnTime" },
                                                            { 24195080, "SQPulseSpecificBrachyControlPointDeliveredSequence" },
                                                            { 24260616, "USPulseNumber" },
                                                            { 24326152, "SQBrachyPulseControlPointDeliveredSequence" },
                                                            { 33566728, "CSCurrentTreatmentStatus" },
                                                            { 33697800, "STTreatmentStatusComment" },
                                                            { 35663880, "SQFractionGroupSummarySequence" },
                                                            { 35860488, "ISReferencedFractionNumber" },
                                                            { 35926024, "CSFractionGroupType" },
                                                            { 36712456, "CSBeamStopperPosition" },
                                                            { 37761032, "SQFractionStatusSummarySequence" },
                                                            { 38809608, "DATreatmentDate" },
                                                            { 38875144, "TMTreatmentTime" },
                                                            { 143370, "SHRTPlanLabel" },
                                                            { 208906, "LORTPlanName" },
                                                            { 274442, "STRTPlanDescription" },
                                                            { 405514, "DARTPlanDate" },
                                                            { 471050, "TMRTPlanTime" },
                                                            { 602122, "LOTreatmentProtocols" },
                                                            { 667658, "CSPlanIntent" },
                                                            { 733194, "LOTreatmentSites" },
                                                            { 798730, "CSRTPlanGeometry" },
                                                            { 929802, "STPrescriptionDescription" },
                                                            { 1060874, "SQDoseReferenceSequence" },
                                                            { 1191946, "ISDoseReferenceNumber" },
                                                            { 1257482, "UIDoseReferenceUID" },
                                                            { 1323018, "CSDoseReferenceStructureType" },
                                                            { 1388554, "CSNominalBeamEnergyUnit" },
                                                            { 1454090, "LODoseReferenceDescription" },
                                                            { 1585162, "DSDoseReferencePointCoordinates" },
                                                            { 1716234, "DSNominalPriorDose" },
                                                            { 2109450, "CSDoseReferenceType" },
                                                            { 2174986, "DSConstraintWeight" },
                                                            { 2240522, "DSDeliveryWarningDose" },
                                                            { 2306058, "DSDeliveryMaximumDose" },
                                                            { 2437130, "DSTargetMinimumDose" },
                                                            { 2502666, "DSTargetPrescriptionDose" },
                                                            { 2568202, "DSTargetMaximumDose" },
                                                            { 2633738, "DSTargetUnderdoseVolumeFraction" },
                                                            { 2764810, "DSOrganAtRiskFullVolumeDose" },
                                                            { 2830346, "DSOrganAtRiskLimitDose" },
                                                            { 2895882, "DSOrganAtRiskMaximumDose" },
                                                            { 2961418, "DSOrganAtRiskOverdoseVolumeFraction" },
                                                            { 4206602, "SQToleranceTableSequence" },
                                                            { 4337674, "ISToleranceTableNumber" },
                                                            { 4403210, "SHToleranceTableLabel" },
                                                            { 4468746, "DSGantryAngleTolerance" },
                                                            { 4599818, "DSBeamLimitingDeviceAngleTolerance" },
                                                            { 4730890, "SQBeamLimitingDeviceToleranceSequence" },
                                                            { 4861962, "DSBeamLimitingDevicePositionTolerance" },
                                                            { 4927498, "FLSnoutPositionTolerance" },
                                                            { 4993034, "DSPatientSupportAngleTolerance" },
                                                            { 5124106, "DSTableTopEccentricAngleTolerance" },
                                                            { 5189642, "FLTableTopPitchAngleTolerance" },
                                                            { 5255178, "FLTableTopRollAngleTolerance" },
                                                            { 5320714, "DSTableTopVerticalPositionTolerance" },
                                                            { 5386250, "DSTableTopLongitudinalPositionTolerance" },
                                                            { 5451786, "DSTableTopLateralPositionTolerance" },
                                                            { 5582858, "CSRTPlanRelationship" },
                                                            { 7352330, "SQFractionGroupSequence" },
                                                            { 7417866, "ISFractionGroupNumber" },
                                                            { 7483402, "LOFractionGroupDescription" },
                                                            { 7876618, "ISNumberOfFractionsPlanned" },
                                                            { 7942154, "ISNumberOfFractionPatternDigitsPerDay" },
                                                            { 8007690, "ISRepeatFractionCycleLength" },
                                                            { 8073226, "LTFractionPattern" },
                                                            { 8400906, "ISNumberOfBeams" },
                                                            { 8531978, "DSBeamDoseSpecificationPoint" },
                                                            { 8663050, "DSBeamDose" },
                                                            { 8794122, "DSBeamMeterset" },
                                                            { 8925194, "FLBeamDosePointDepth" },
                                                            { 8990730, "FLBeamDosePointEquivalentDepth" },
                                                            { 9056266, "FLBeamDosePointSSD" },
                                                            { 9121802, "CSBeamDoseMeaning" },
                                                            { 9187338, "SQBeamDoseVerificationControlPointSequence" },
                                                            { 9252874, "FLAverageBeamDosePointDepth" },
                                                            { 9318410, "FLAverageBeamDosePointEquivalentDepth" },
                                                            { 9383946, "FLAverageBeamDosePointSSD" },
                                                            { 9449482, "CSBeamDoseType" },
                                                            { 9515018, "DSAlternateBeamDose" },
                                                            { 9580554, "CSAlternateBeamDoseType" },
                                                            { 10498058, "ISNumberOfBrachyApplicationSetups" },
                                                            { 10629130, "DSBrachyApplicationSetupDoseSpecificationPoint" },
                                                            { 10760202, "DSBrachyApplicationSetupDose" },
                                                            { 11546634, "SQBeamSequence" },
                                                            { 11677706, "SHTreatmentMachineName" },
                                                            { 11743242, "CSPrimaryDosimeterUnit" },
                                                            { 11808778, "DSSourceAxisDistance" },
                                                            { 11939850, "SQBeamLimitingDeviceSequence" },
                                                            { 12070922, "CSRTBeamLimitingDeviceType" },
                                                            { 12201994, "DSSourceToBeamLimitingDeviceDistance" },
                                                            { 12267530, "FLIsocenterToBeamLimitingDeviceDistance" },
                                                            { 12333066, "ISNumberOfLeafJawPairs" },
                                                            { 12464138, "DSLeafPositionBoundaries" },
                                                            { 12595210, "ISBeamNumber" },
                                                            { 12726282, "LOBeamName" },
                                                            { 12791818, "STBeamDescription" },
                                                            { 12857354, "CSBeamType" },
                                                            { 12922890, "FDBeamDeliveryDurationLimit" },
                                                            { 12988426, "CSRadiationType" },
                                                            { 13053962, "CSHighDoseTechniqueType" },
                                                            { 13119498, "ISReferenceImageNumber" },
                                                            { 13250570, "SQPlannedVerificationImageSequence" },
                                                            { 13381642, "LOImagingDeviceSpecificAcquisitionParameters" },
                                                            { 13512714, "CSTreatmentDeliveryType" },
                                                            { 13643786, "ISNumberOfWedges" },
                                                            { 13709322, "SQWedgeSequence" },
                                                            { 13774858, "ISWedgeNumber" },
                                                            { 13840394, "CSWedgeType" },
                                                            { 13905930, "SHWedgeID" },
                                                            { 13971466, "ISWedgeAngle" },
                                                            { 14037002, "DSWedgeFactor" },
                                                            { 14102538, "FLTotalWedgeTrayWaterEquivalentThickness" },
                                                            { 14168074, "DSWedgeOrientation" },
                                                            { 14233610, "FLIsocenterToWedgeTrayDistance" },
                                                            { 14299146, "DSSourceToWedgeTrayDistance" },
                                                            { 14364682, "FLWedgeThinEdgePosition" },
                                                            { 14430218, "SHBolusID" },
                                                            { 14495754, "STBolusDescription" },
                                                            { 14561290, "DSEffectiveWedgeAngle" },
                                                            { 14692362, "ISNumberOfCompensators" },
                                                            { 14757898, "SHMaterialID" },
                                                            { 14823434, "DSTotalCompensatorTrayFactor" },
                                                            { 14888970, "SQCompensatorSequence" },
                                                            { 14954506, "ISCompensatorNumber" },
                                                            { 15020042, "SHCompensatorID" },
                                                            { 15085578, "DSSourceToCompensatorTrayDistance" },
                                                            { 15151114, "ISCompensatorRows" },
                                                            { 15216650, "ISCompensatorColumns" },
                                                            { 15282186, "DSCompensatorPixelSpacing" },
                                                            { 15347722, "DSCompensatorPosition" },
                                                            { 15413258, "DSCompensatorTransmissionData" },
                                                            { 15478794, "DSCompensatorThicknessData" },
                                                            { 15544330, "ISNumberOfBoli" },
                                                            { 15609866, "CSCompensatorType" },
                                                            { 15675402, "SHCompensatorTrayID" },
                                                            { 15740938, "ISNumberOfBlocks" },
                                                            { 15872010, "DSTotalBlockTrayFactor" },
                                                            { 15937546, "FLTotalBlockTrayWaterEquivalentThickness" },
                                                            { 16003082, "SQBlockSequence" },
                                                            { 16068618, "SHBlockTrayID" },
                                                            { 16134154, "DSSourceToBlockTrayDistance" },
                                                            { 16199690, "FLIsocenterToBlockTrayDistance" },
                                                            { 16265226, "CSBlockType" },
                                                            { 16330762, "LOAccessoryCode" },
                                                            { 16396298, "CSBlockDivergence" },
                                                            { 16461834, "CSBlockMountingPosition" },
                                                            { 16527370, "ISBlockNumber" },
                                                            { 16658442, "LOBlockName" },
                                                            { 16789514, "DSBlockThickness" },
                                                            { 16920586, "DSBlockTransmission" },
                                                            { 17051658, "ISBlockNumberOfPoints" },
                                                            { 17182730, "DSBlockData" },
                                                            { 17248266, "SQApplicatorSequence" },
                                                            { 17313802, "SHApplicatorID" },
                                                            { 17379338, "CSApplicatorType" },
                                                            { 17444874, "LOApplicatorDescription" },
                                                            { 17575946, "DSCumulativeDoseReferenceCoefficient" },
                                                            { 17707018, "DSFinalCumulativeMetersetWeight" },
                                                            { 17838090, "ISNumberOfControlPoints" },
                                                            { 17903626, "SQControlPointSequence" },
                                                            { 17969162, "ISControlPointIndex" },
                                                            { 18100234, "DSNominalBeamEnergy" },
                                                            { 18165770, "DSDoseRateSet" },
                                                            { 18231306, "SQWedgePositionSequence" },
                                                            { 18362378, "CSWedgePosition" },
                                                            { 18493450, "SQBeamLimitingDevicePositionSequence" },
                                                            { 18624522, "DSLeafJawPositions" },
                                                            { 18755594, "DSGantryAngle" },
                                                            { 18821130, "CSGantryRotationDirection" },
                                                            { 18886666, "DSBeamLimitingDeviceAngle" },
                                                            { 18952202, "CSBeamLimitingDeviceRotationDirection" },
                                                            { 19017738, "DSPatientSupportAngle" },
                                                            { 19083274, "CSPatientSupportRotationDirection" },
                                                            { 19148810, "DSTableTopEccentricAxisDistance" },
                                                            { 19214346, "DSTableTopEccentricAngle" },
                                                            { 19279882, "CSTableTopEccentricRotationDirection" },
                                                            { 19410954, "DSTableTopVerticalPosition" },
                                                            { 19476490, "DSTableTopLongitudinalPosition" },
                                                            { 19542026, "DSTableTopLateralPosition" },
                                                            { 19673098, "DSIsocenterPosition" },
                                                            { 19804170, "DSSurfaceEntryPoint" },
                                                            { 19935242, "DSSourceToSurfaceDistance" },
                                                            { 20000778, "FLAverageBeamDosePointSourceToExternalContourDistance" },
                                                            { 20066314, "FLSourceToExternalContourDistance" },
                                                            { 20131850, "FLExternalContourEntryPoint" },
                                                            { 20197386, "DSCumulativeMetersetWeight" },
                                                            { 20983818, "FLTableTopPitchAngle" },
                                                            { 21114890, "CSTableTopPitchRotationDirection" },
                                                            { 21245962, "FLTableTopRollAngle" },
                                                            { 21377034, "CSTableTopRollRotationDirection" },
                                                            { 21508106, "FLHeadFixationAngle" },
                                                            { 21639178, "FLGantryPitchAngle" },
                                                            { 21770250, "CSGantryPitchRotationDirection" },
                                                            { 21901322, "FLGantryPitchAngleTolerance" },
                                                            { 22032394, "CSFixationEye" },
                                                            { 22097930, "DSChairHeadFramePosition" },
                                                            { 22163466, "DSHeadFixationAngleTolerance" },
                                                            { 22229002, "DSChairHeadFramePositionTolerance" },
                                                            { 22294538, "DSFixationLightAzimuthalAngleTolerance" },
                                                            { 22360074, "DSFixationLightPolarAngleTolerance" },
                                                            { 25178122, "SQPatientSetupSequence" },
                                                            { 25309194, "ISPatientSetupNumber" },
                                                            { 25374730, "LOPatientSetupLabel" },
                                                            { 25440266, "LOPatientAdditionalPosition" },
                                                            { 26226698, "SQFixationDeviceSequence" },
                                                            { 26357770, "CSFixationDeviceType" },
                                                            { 26488842, "SHFixationDeviceLabel" },
                                                            { 26619914, "STFixationDeviceDescription" },
                                                            { 26750986, "SHFixationDevicePosition" },
                                                            { 26816522, "FLFixationDevicePitchAngle" },
                                                            { 26882058, "FLFixationDeviceRollAngle" },
                                                            { 27275274, "SQShieldingDeviceSequence" },
                                                            { 27406346, "CSShieldingDeviceType" },
                                                            { 27537418, "SHShieldingDeviceLabel" },
                                                            { 27668490, "STShieldingDeviceDescription" },
                                                            { 27799562, "SHShieldingDevicePosition" },
                                                            { 28323850, "CSSetupTechnique" },
                                                            { 28454922, "STSetupTechniqueDescription" },
                                                            { 28585994, "SQSetupDeviceSequence" },
                                                            { 28717066, "CSSetupDeviceType" },
                                                            { 28848138, "SHSetupDeviceLabel" },
                                                            { 28979210, "STSetupDeviceDescription" },
                                                            { 29110282, "DSSetupDeviceParameter" },
                                                            { 30421002, "STSetupReferenceDescription" },
                                                            { 30552074, "DSTableTopVerticalSetupDisplacement" },
                                                            { 30683146, "DSTableTopLongitudinalSetupDisplacement" },
                                                            { 30814218, "DSTableTopLateralSetupDisplacement" },
                                                            { 33566730, "CSBrachyTreatmentTechnique" },
                                                            { 33697802, "CSBrachyTreatmentType" },
                                                            { 33959946, "SQTreatmentMachineSequence" },
                                                            { 34615306, "SQSourceSequence" },
                                                            { 34746378, "ISSourceNumber" },
                                                            { 34877450, "CSSourceType" },
                                                            { 35008522, "LOSourceManufacturer" },
                                                            { 35139594, "DSActiveSourceDiameter" },
                                                            { 35270666, "DSActiveSourceLength" },
                                                            { 35336202, "SHSourceModelID" },
                                                            { 35401738, "LOSourceDescription" },
                                                            { 35794954, "DSSourceEncapsulationNominalThickness" },
                                                            { 35926026, "DSSourceEncapsulationNominalTransmission" },
                                                            { 36057098, "LOSourceIsotopeName" },
                                                            { 36188170, "DSSourceIsotopeHalfLife" },
                                                            { 36253706, "CSSourceStrengthUnits" },
                                                            { 36319242, "DSReferenceAirKermaRate" },
                                                            { 36384778, "DSSourceStrength" },
                                                            { 36450314, "DASourceStrengthReferenceDate" },
                                                            { 36581386, "TMSourceStrengthReferenceTime" },
                                                            { 36712458, "SQApplicationSetupSequence" },
                                                            { 36843530, "CSApplicationSetupType" },
                                                            { 36974602, "ISApplicationSetupNumber" },
                                                            { 37105674, "LOApplicationSetupName" },
                                                            { 37236746, "LOApplicationSetupManufacturer" },
                                                            { 37761034, "ISTemplateNumber" },
                                                            { 37892106, "SHTemplateType" },
                                                            { 38023178, "LOTemplateName" },
                                                            { 38809610, "DSTotalReferenceAirKerma" },
                                                            { 39858186, "SQBrachyAccessoryDeviceSequence" },
                                                            { 39989258, "ISBrachyAccessoryDeviceNumber" },
                                                            { 40054794, "SHBrachyAccessoryDeviceID" },
                                                            { 40120330, "CSBrachyAccessoryDeviceType" },
                                                            { 40251402, "LOBrachyAccessoryDeviceName" },
                                                            { 40513546, "DSBrachyAccessoryDeviceNominalThickness" },
                                                            { 40644618, "DSBrachyAccessoryDeviceNominalTransmission" },
                                                            { 41955338, "SQChannelSequence" },
                                                            { 42086410, "ISChannelNumber" },
                                                            { 42217482, "DSChannelLength" },
                                                            { 42348554, "DSChannelTotalTime" },
                                                            { 42479626, "CSSourceMovementType" },
                                                            { 42610698, "ISNumberOfPulses" },
                                                            { 42741770, "DSPulseRepetitionInterval" },
                                                            { 43003914, "ISSourceApplicatorNumber" },
                                                            { 43069450, "SHSourceApplicatorID" },
                                                            { 43134986, "CSSourceApplicatorType" },
                                                            { 43266058, "LOSourceApplicatorName" },
                                                            { 43397130, "DSSourceApplicatorLength" },
                                                            { 43528202, "LOSourceApplicatorManufacturer" },
                                                            { 43790346, "DSSourceApplicatorWallNominalThickness" },
                                                            { 43921418, "DSSourceApplicatorWallNominalTransmission" },
                                                            { 44052490, "DSSourceApplicatorStepSize" },
                                                            { 44183562, "ISTransferTubeNumber" },
                                                            { 44314634, "DSTransferTubeLength" },
                                                            { 45101066, "SQChannelShieldSequence" },
                                                            { 45232138, "ISChannelShieldNumber" },
                                                            { 45297674, "SHChannelShieldID" },
                                                            { 45363210, "LOChannelShieldName" },
                                                            { 45625354, "DSChannelShieldNominalThickness" },
                                                            { 45756426, "DSChannelShieldNominalTransmission" },
                                                            { 46673930, "DSFinalCumulativeTimeWeight" },
                                                            { 47198218, "SQBrachyControlPointSequence" },
                                                            { 47329290, "DSControlPointRelativePosition" },
                                                            { 47460362, "DSControlPoint3DPosition" },
                                                            { 47591434, "DSCumulativeTimeWeight" },
                                                            { 48246794, "CSCompensatorDivergence" },
                                                            { 48312330, "CSCompensatorMountingPosition" },
                                                            { 48377866, "DSSourceToCompensatorDistance" },
                                                            { 48443402, "FLTotalCompensatorTrayWaterEquivalentThickness" },
                                                            { 48508938, "FLIsocenterToCompensatorTrayDistance" },
                                                            { 48574474, "FLCompensatorColumnOffset" },
                                                            { 48640010, "FLIsocenterToCompensatorDistances" },
                                                            { 48705546, "FLCompensatorRelativeStoppingPowerRatio" },
                                                            { 48771082, "FLCompensatorMillingToolDiameter" },
                                                            { 48902154, "SQIonRangeCompensatorSequence" },
                                                            { 48967690, "LTCompensatorDescription" },
                                                            { 50475018, "ISRadiationMassNumber" },
                                                            { 50606090, "ISRadiationAtomicNumber" },
                                                            { 50737162, "SSRadiationChargeState" },
                                                            { 50868234, "CSScanMode" },
                                                            { 50933770, "CSModulatedScanModeType" },
                                                            { 50999306, "FLVirtualSourceAxisDistances" },
                                                            { 51130378, "SQSnoutSequence" },
                                                            { 51195914, "FLSnoutPosition" },
                                                            { 51326986, "SHSnoutID" },
                                                            { 51523594, "ISNumberOfRangeShifters" },
                                                            { 51654666, "SQRangeShifterSequence" },
                                                            { 51785738, "ISRangeShifterNumber" },
                                                            { 51916810, "SHRangeShifterID" },
                                                            { 52441098, "CSRangeShifterType" },
                                                            { 52572170, "LORangeShifterDescription" },
                                                            { 53489674, "ISNumberOfLateralSpreadingDevices" },
                                                            { 53620746, "SQLateralSpreadingDeviceSequence" },
                                                            { 53751818, "ISLateralSpreadingDeviceNumber" },
                                                            { 53882890, "SHLateralSpreadingDeviceID" },
                                                            { 54013962, "CSLateralSpreadingDeviceType" },
                                                            { 54145034, "LOLateralSpreadingDeviceDescription" },
                                                            { 54276106, "FLLateralSpreadingDeviceWaterEquivalentThickness" },
                                                            { 54538250, "ISNumberOfRangeModulators" },
                                                            { 54669322, "SQRangeModulatorSequence" },
                                                            { 54800394, "ISRangeModulatorNumber" },
                                                            { 54931466, "SHRangeModulatorID" },
                                                            { 55062538, "CSRangeModulatorType" },
                                                            { 55193610, "LORangeModulatorDescription" },
                                                            { 55324682, "SHBeamCurrentModulationID" },
                                                            { 55586826, "CSPatientSupportType" },
                                                            { 55717898, "SHPatientSupportID" },
                                                            { 55848970, "LOPatientSupportAccessoryCode" },
                                                            { 55914506, "LOTrayAccessoryCode" },
                                                            { 55980042, "FLFixationLightAzimuthalAngle" },
                                                            { 56111114, "FLFixationLightPolarAngle" },
                                                            { 56242186, "FLMetersetRate" },
                                                            { 56635402, "SQRangeShifterSettingsSequence" },
                                                            { 56766474, "LORangeShifterSetting" },
                                                            { 56897546, "FLIsocenterToRangeShifterDistance" },
                                                            { 57028618, "FLRangeShifterWaterEquivalentThickness" },
                                                            { 57683978, "SQLateralSpreadingDeviceSettingsSequence" },
                                                            { 57815050, "LOLateralSpreadingDeviceSetting" },
                                                            { 57946122, "FLIsocenterToLateralSpreadingDeviceDistance" },
                                                            { 58732554, "SQRangeModulatorSettingsSequence" },
                                                            { 58863626, "FLRangeModulatorGatingStartValue" },
                                                            { 58994698, "FLRangeModulatorGatingStopValue" },
                                                            { 59125770, "FLRangeModulatorGatingStartWaterEquivalentThickness" },
                                                            { 59256842, "FLRangeModulatorGatingStopWaterEquivalentThickness" },
                                                            { 59387914, "FLIsocenterToRangeModulatorDistance" },
                                                            { 59715594, "FLScanSpotTimeOffset" },
                                                            { 59781130, "SHScanSpotTuneID" },
                                                            { 59846666, "ISScanSpotPrescribedIndices" },
                                                            { 59912202, "ISNumberOfScanSpotPositions" },
                                                            { 59977738, "CSScanSpotReordered" },
                                                            { 60043274, "FLScanSpotPositionMap" },
                                                            { 60108810, "CSScanSpotReorderingAllowed" },
                                                            { 60174346, "FLScanSpotMetersetWeights" },
                                                            { 60305418, "FLScanningSpotSize" },
                                                            { 60436490, "ISNumberOfPaintings" },
                                                            { 60829706, "SQIonToleranceTableSequence" },
                                                            { 60960778, "SQIonBeamSequence" },
                                                            { 61091850, "SQIonBeamLimitingDeviceSequence" },
                                                            { 61222922, "SQIonBlockSequence" },
                                                            { 61353994, "SQIonControlPointSequence" },
                                                            { 61485066, "SQIonWedgeSequence" },
                                                            { 61616138, "SQIonWedgePositionSequence" },
                                                            { 67186698, "SQReferencedSetupImageSequence" },
                                                            { 67252234, "STSetupImageComment" },
                                                            { 68169738, "SQMotionSynchronizationSequence" },
                                                            { 68300810, "FLControlPointOrientation" },
                                                            { 69218314, "SQGeneralAccessorySequence" },
                                                            { 69283850, "SHGeneralAccessoryID" },
                                                            { 69349386, "STGeneralAccessoryDescription" },
                                                            { 69414922, "CSGeneralAccessoryType" },
                                                            { 69480458, "ISGeneralAccessoryNumber" },
                                                            { 69545994, "FLSourceToGeneralAccessoryDistance" },
                                                            { 70332426, "SQApplicatorGeometrySequence" },
                                                            { 70397962, "CSApplicatorApertureShape" },
                                                            { 70463498, "FLApplicatorOpening" },
                                                            { 70529034, "FLApplicatorOpeningX" },
                                                            { 70594570, "FLApplicatorOpeningY" },
                                                            { 70660106, "FLSourceToApplicatorMountingPositionDistance" },
                                                            { 71315466, "ISNumberOfBlockSlabItems" },
                                                            { 71381002, "SQBlockSlabSequence" },
                                                            { 71446538, "DSBlockSlabThickness" },
                                                            { 71512074, "USBlockSlabNumber" },
                                                            { 72364042, "SQDeviceMotionControlSequence" },
                                                            { 72429578, "CSDeviceMotionExecutionMode" },
                                                            { 72495114, "CSDeviceMotionObservationMode" },
                                                            { 72560650, "SQDeviceMotionParameterCodeSequence" },
                                                            { 83963914, "FLDistalDepthFraction" },
                                                            { 84029450, "FLDistalDepth" },
                                                            { 84094986, "FLNominalRangeModulationFractions" },
                                                            { 84160522, "FLNominalRangeModulatedRegionDepths" },
                                                            { 84226058, "SQDepthDoseParametersSequence" },
                                                            { 84291594, "SQDeliveredDepthDoseParametersSequence" },
                                                            { 84357130, "FLDeliveredDistalDepthFraction" },
                                                            { 84422666, "FLDeliveredDistalDepth" },
                                                            { 84488202, "FLDeliveredNominalRangeModulationFractions" },
                                                            { 84946954, "FLDeliveredNominalRangeModulatedRegionDepths" },
                                                            { 85012490, "CSDeliveredReferenceDoseDefinition" },
                                                            { 85078026, "CSReferenceDoseDefinition" },
                                                            { 143372, "SQReferencedRTPlanSequence" },
                                                            { 274444, "SQReferencedBeamSequence" },
                                                            { 405516, "ISReferencedBeamNumber" },
                                                            { 471052, "ISReferencedReferenceImageNumber" },
                                                            { 536588, "DSStartCumulativeMetersetWeight" },
                                                            { 602124, "DSEndCumulativeMetersetWeight" },
                                                            { 667660, "SQReferencedBrachyApplicationSetupSequence" },
                                                            { 798732, "ISReferencedBrachyApplicationSetupNumber" },
                                                            { 929804, "ISReferencedSourceNumber" },
                                                            { 2109452, "SQReferencedFractionGroupSequence" },
                                                            { 2240524, "ISReferencedFractionGroupNumber" },
                                                            { 4206604, "SQReferencedVerificationImageSequence" },
                                                            { 4337676, "SQReferencedReferenceImageSequence" },
                                                            { 5255180, "SQReferencedDoseReferenceSequence" },
                                                            { 5320716, "ISReferencedDoseReferenceNumber" },
                                                            { 5582860, "SQBrachyReferencedDoseReferenceSequence" },
                                                            { 6303756, "SQReferencedStructureSetSequence" },
                                                            { 6959116, "ISReferencedPatientSetupNumber" },
                                                            { 8400908, "SQReferencedDoseSequence" },
                                                            { 10498060, "ISReferencedToleranceTableNumber" },
                                                            { 11546636, "SQReferencedBolusSequence" },
                                                            { 12595212, "ISReferencedWedgeNumber" },
                                                            { 13643788, "ISReferencedCompensatorNumber" },
                                                            { 14692364, "ISReferencedBlockNumber" },
                                                            { 15740940, "ISReferencedControlPointIndex" },
                                                            { 15872012, "SQReferencedControlPointSequence" },
                                                            { 16003084, "ISReferencedStartControlPointIndex" },
                                                            { 16134156, "ISReferencedStopControlPointIndex" },
                                                            { 16789516, "ISReferencedRangeShifterNumber" },
                                                            { 16920588, "ISReferencedLateralSpreadingDeviceNumber" },
                                                            { 17051660, "ISReferencedRangeModulatorNumber" },
                                                            { 17903628, "SQOmittedBeamTaskSequence" },
                                                            { 17969164, "CSReasonForOmission" },
                                                            { 18034700, "LOReasonForOmissionDescription" },
                                                            { 143374, "CSApprovalStatus" },
                                                            { 274446, "DAReviewDate" },
                                                            { 339982, "TMReviewTime" },
                                                            { 536590, "PNReviewerName" },
                                                            { 1064960, "LTArbitrary" },
                                                            { 1073758208, "LTTextComments" },
                                                            { 4210696, "SHResultsID" },
                                                            { 4341768, "LOResultsIDIssuer" },
                                                            { 5259272, "SQReferencedInterpretationSequence" },
                                                            { 16728072, "CSReportProductionStatusTrial" },
                                                            { 16793608, "DAInterpretationRecordedDate" },
                                                            { 16859144, "TMInterpretationRecordedTime" },
                                                            { 16924680, "PNInterpretationRecorder" },
                                                            { 16990216, "LOReferenceToRecordedSound" },
                                                            { 17317896, "DAInterpretationTranscriptionDate" },
                                                            { 17383432, "TMInterpretationTranscriptionTime" },
                                                            { 17448968, "PNInterpretationTranscriber" },
                                                            { 17514504, "STInterpretationText" },
                                                            { 17580040, "PNInterpretationAuthor" },
                                                            { 17907720, "SQInterpretationApproverSequence" },
                                                            { 17973256, "DAInterpretationApprovalDate" },
                                                            { 18038792, "TMInterpretationApprovalTime" },
                                                            { 18104328, "PNPhysicianApprovingInterpretation" },
                                                            { 18169864, "LTInterpretationDiagnosisDescription" },
                                                            { 18300936, "SQInterpretationDiagnosisCodeSequence" },
                                                            { 18366472, "SQResultsDistributionListSequence" },
                                                            { 18432008, "PNDistributionName" },
                                                            { 18497544, "LODistributionAddress" },
                                                            { 33570824, "SHInterpretationID" },
                                                            { 33701896, "LOInterpretationIDIssuer" },
                                                            { 34619400, "CSInterpretationTypeID" },
                                                            { 34750472, "CSInterpretationStatusID" },
                                                            { 50348040, "STImpressions" },
                                                            { 1073758216, "STResultsComments" },
                                                            { 81936, "CSLowEnergyDetectors" },
                                                            { 147472, "CSHighEnergyDetectors" },
                                                            { 278544, "SQDetectorGeometrySequence" },
                                                            { 268517392, "SQThreatROIVoxelSequence" },
                                                            { 268714000, "FLThreatROIBase" },
                                                            { 268779536, "FLThreatROIExtents" },
                                                            { 268845072, "OBThreatROIBitmap" },
                                                            { 268910608, "SHRouteSegmentID" },
                                                            { 268976144, "CSGantryType" },
                                                            { 269041680, "CSOOIOwnerType" },
                                                            { 269107216, "SQRouteSegmentSequence" },
                                                            { 269500432, "USPotentialThreatObjectID" },
                                                            { 269565968, "SQThreatSequence" },
                                                            { 269631504, "CSThreatCategory" },
                                                            { 269697040, "LTThreatCategoryDescription" },
                                                            { 269762576, "CSATDAbilityAssessment" },
                                                            { 269828112, "CSATDAssessmentFlag" },
                                                            { 269893648, "FLATDAssessmentProbability" },
                                                            { 269959184, "FLMass" },
                                                            { 270024720, "FLDensity" },
                                                            { 270090256, "FLZEffective" },
                                                            { 270155792, "SHBoardingPassID" },
                                                            { 270221328, "FLCenterOfMass" },
                                                            { 270286864, "FLCenterOfPTO" },
                                                            { 270352400, "FLBoundingPolygon" },
                                                            { 270417936, "SHRouteSegmentStartLocationID" },
                                                            { 270483472, "SHRouteSegmentEndLocationID" },
                                                            { 270549008, "CSRouteSegmentLocationIDType" },
                                                            { 270614544, "CSAbortReason" },
                                                            { 270745616, "FLVolumeOfPTO" },
                                                            { 270811152, "CSAbortFlag" },
                                                            { 270876688, "DTRouteSegmentStartTime" },
                                                            { 270942224, "DTRouteSegmentEndTime" },
                                                            { 271007760, "CSTDRType" },
                                                            { 271073296, "CSInternationalRouteSegment" },
                                                            { 271138832, "LOThreatDetectionAlgorithmandVersion" },
                                                            { 271204368, "SHAssignedLocation" },
                                                            { 271269904, "DTAlarmDecisionTime" },
                                                            { 271663120, "CSAlarmDecision" },
                                                            { 271794192, "USNumberOfTotalObjects" },
                                                            { 271859728, "USNumberOfAlarmObjects" },
                                                            { 272056336, "SQPTORepresentationSequence" },
                                                            { 272121872, "SQATDAssessmentSequence" },
                                                            { 272187408, "CSTIPType" },
                                                            { 272252944, "CSDICOSVersion" },
                                                            { 272711696, "DTOOIOwnerCreationTime" },
                                                            { 272777232, "CSOOIType" },
                                                            { 272842768, "FLOOISize" },
                                                            { 272908304, "CSAcquisitionStatus" },
                                                            { 272973840, "SQBasisMaterialsCodeSequence" },
                                                            { 273039376, "CSPhantomType" },
                                                            { 273104912, "SQOOIOwnerSequence" },
                                                            { 273170448, "CSScanType" },
                                                            { 273760272, "LOItineraryID" },
                                                            { 273825808, "SHItineraryIDType" },
                                                            { 273891344, "LOItineraryIDAssigningAuthority" },
                                                            { 273956880, "SHRouteID" },
                                                            { 274022416, "SHRouteIDAssigningAuthority" },
                                                            { 274087952, "CSInboundArrivalType" },
                                                            { 274219024, "SHCarrierID" },
                                                            { 274284560, "CSCarrierIDAssigningAuthority" },
                                                            { 274743312, "FLSourceOrientation" },
                                                            { 274808848, "FLSourcePosition" },
                                                            { 274874384, "FLBeltHeight" },
                                                            { 275005456, "SQAlgorithmRoutingCodeSequence" },
                                                            { 275202064, "CSTransportClassification" },
                                                            { 275267600, "LTOOITypeDescriptor" },
                                                            { 275333136, "FLTotalProcessingTime" },
                                                            { 275529744, "OBDetectorCalibrationData" },
                                                            { 275595280, "CSAdditionalScreeningPerformed" },
                                                            { 275660816, "CSAdditionalInspectionSelectionCriteria" },
                                                            { 275726352, "SQAdditionalInspectionMethodSequence" },
                                                            { 275791888, "CSAITDeviceType" },
                                                            { 275857424, "SQQRMeasurementsSequence" },
                                                            { 275922960, "SQTargetMaterialSequence" },
                                                            { 275988496, "FDSNRThreshold" },
                                                            { 276119568, "DSImageScaleRepresentation" },
                                                            { 276185104, "SQReferencedPTOSequence" },
                                                            { 276250640, "SQReferencedTDRInstanceSequence" },
                                                            { 276316176, "STPTOLocationDescription" },
                                                            { 276381712, "SQAnomalyLocatorIndicatorSequence" },
                                                            { 276447248, "FLAnomalyLocatorIndicator" },
                                                            { 276512784, "SQPTORegionSequence" },
                                                            { 276578320, "CSInspectionSelectionCriteria" },
                                                            { 276643856, "SQSecondaryInspectionMethodSequence" },
                                                            { 276709392, "DSPRCSToRCSOrientation" },
                                                            { 86014, "SQMACParametersSequence" },
                                                            { 348160, "USCurveDimensions" },
                                                            { 1069056, "USNumberOfPoints" },
                                                            { 2117632, "CSTypeOfData" },
                                                            { 2248704, "LOCurveDescription" },
                                                            { 3166208, "SHAxisUnits" },
                                                            { 4214784, "SHAxisLabels" },
                                                            { 16994304, "USDataValueRepresentation" },
                                                            { 17059840, "USMinimumCoordinateValue" },
                                                            { 17125376, "USMaximumCoordinateValue" },
                                                            { 17190912, "SHCurveRange" },
                                                            { 17846272, "USCurveDataDescriptor" },
                                                            { 17977344, "USCoordinateStartValue" },
                                                            { 18108416, "USCoordinateStepValue" },
                                                            { 268521472, "CSCurveActivationLayer" },
                                                            { 536891392, "USAudioType" },
                                                            { 537022464, "USAudioSampleFormat" },
                                                            { 537153536, "USNumberOfChannels" },
                                                            { 537284608, "ULNumberOfSamples" },
                                                            { 537415680, "ULSampleRate" },
                                                            { 537546752, "ULTotalTime" },
                                                            { 537677824, "OBAudioSampleData" },
                                                            { 537808896, "LTAudioComments" },
                                                            { 620777472, "LOCurveLabel" },
                                                            { 637554688, "SQCurveReferencedOverlaySequence" },
                                                            { 638603264, "USCurveReferencedOverlayGroup" },
                                                            { 805326848, "OBCurveData" },
                                                            { 2452181504, "SQSharedFunctionalGroupsSequence" },
                                                            { 2452640256, "SQPerFrameFunctionalGroupsSequence" },
                                                            { 16798720, "SQWaveformSequence" },
                                                            { 17847296, "OBChannelMinimumValue" },
                                                            { 17978368, "OBChannelMaximumValue" },
                                                            { 268719104, "USWaveformBitsAllocated" },
                                                            { 268850176, "CSWaveformSampleInterpretation" },
                                                            { 269112320, "OBWaveformPaddingValue" },
                                                            { 269505536, "OBWaveformData" },
                                                            { 1070592, "OFFirstOrderPhaseCorrectionAngle" },
                                                            { 2119168, "OFSpectroscopyData" },
                                                            { 1073152, "USOverlayRows" },
                                                            { 1138688, "USOverlayColumns" },
                                                            { 1204224, "USOverlayPlanes" },
                                                            { 1400832, "ISNumberOfFramesInOverlay" },
                                                            { 2252800, "LOOverlayDescription" },
                                                            { 4218880, "CSOverlayType" },
                                                            { 4546560, "LOOverlaySubtype" },
                                                            { 5267456, "SSOverlayOrigin" },
                                                            { 5332992, "USImageFrameOrigin" },
                                                            { 5398528, "USOverlayPlaneOrigin" },
                                                            { 6316032, "CSOverlayCompressionCode" },
                                                            { 6381568, "SHOverlayCompressionOriginator" },
                                                            { 6447104, "SHOverlayCompressionLabel" },
                                                            { 6512640, "CSOverlayCompressionDescription" },
                                                            { 6709248, "ATOverlayCompressionStepPointers" },
                                                            { 6840320, "USOverlayRepeatInterval" },
                                                            { 6905856, "USOverlayBitsGrouped" },
                                                            { 16801792, "USOverlayBitsAllocated" },
                                                            { 16932864, "USOverlayBitPosition" },
                                                            { 17850368, "CSOverlayFormat" },
                                                            { 33579008, "USOverlayLocation" },
                                                            { 134242304, "CSOverlayCodeLabel" },
                                                            { 134373376, "USOverlayNumberOfTables" },
                                                            { 134438912, "ATOverlayCodeTableLocation" },
                                                            { 134504448, "USOverlayBitsForCodeWord" },
                                                            { 268525568, "CSOverlayActivationLayer" },
                                                            { 285237248, "USOverlayDescriptorGray" },
                                                            { 285302784, "USOverlayDescriptorRed" },
                                                            { 285368320, "USOverlayDescriptorGreen" },
                                                            { 285433856, "USOverlayDescriptorBlue" },
                                                            { 302014464, "USOverlaysGray" },
                                                            { 302080000, "USOverlaysRed" },
                                                            { 302145536, "USOverlaysGreen" },
                                                            { 302211072, "USOverlaysBlue" },
                                                            { 318857216, "ISROIArea" },
                                                            { 318922752, "DSROIMean" },
                                                            { 318988288, "DSROIStandardDeviation" },
                                                            { 352346112, "LOOverlayLabel" },
                                                            { 805330944, "OBOverlayData" },
                                                            { 1073766400, "LTOverlayComments" },
                                                            { 557024, "OFFloatPixelData" },
                                                            { 622560, "ODDoubleFloatPixelData" },
                                                            { 1081312, "OBPixelData" },
                                                            { 2129888, "OWCoefficientsSDVN" },
                                                            { 3178464, "OWCoefficientsSDHN" },
                                                            { 4227040, "OWCoefficientsSDDN" },
                                                            { 1081088, "OBVariablePixelData" },
                                                            { 1146624, "USVariableNextDataGroup" },
                                                            { 2129664, "OWVariableCoefficientsSDVN" },
                                                            { 3178240, "OWVariableCoefficientsSDHN" },
                                                            { 4226816, "OWVariableCoefficientsSDDN" },
                                                            { 4294639610, "SQDigitalSignaturesSequence" },
                                                            { 4294770684, "OBDataSetTrailingPadding" },
                                                            { 3758161918, "UNItem" },
                                                            { 3759013886, "UNItemDelimitationItem" },
                                                            { 3772645374, "UNSequenceDelimitationItem" }
        };

        public static bool isDICOM(string fName)
        {
            fName = Path.GetFullPath(fName);
            if (!File.Exists(fName))
            {
                return false;
            }
            Int64 fileSize = new System.IO.FileInfo(fName).Length;
            int numBytes2Read = 132;
            if (fileSize < numBytes2Read)
            {
                return false;
            }
            BinaryReader reader;
            byte[] val = null;
            try
            {
                reader = new BinaryReader(File.Open(fName, FileMode.Open));
                if (reader != null)
                {
                    val = reader.ReadBytes(numBytes2Read);
                    reader.BaseStream.Close();
                    reader.Close();
                }
            }
            catch (IOException)                 {   Debug.Log("IOException: " + fName);                     return false; }
            catch (UnauthorizedAccessException) {   Debug.Log("UnauthorizedAccessException: " + fName);     return false; }
            catch (NotSupportedException)       {   Debug.Log("NotSupportedException: " + fName);           return false; }
            catch (ArgumentException)           {   Debug.Log("ArgumentException: " + fName);               return false; }
            if (val.Length < numBytes2Read)
            {
                return false;
            }
            if ((   val[128] == 'D' &&
                    val[129] == 'I' &&
                    val[130] == 'C' &&
                    val[131] == 'M'))
            {
                return true;
            }
            return false;
        }

        public static int getByteOrder()
        {
            return BitConverter.IsLittleEndian ? LSB_FIRST : MSB_FIRST;
        }

        public static void swapBytes(int numElements, int numBytes, ref byte[] array, int startIndex)
        {
            byte temp;
            for (int i = 0; i < numElements; i++)
            {
                for (int j = 0; j < numBytes; j += 2)
                {
                    temp = array[startIndex + i * numBytes + j];
                    array[startIndex + i * numBytes + j] = array[startIndex + (i + 1) * numBytes - j - 1];
                    array[startIndex + (i + 1) * numBytes - j - 1] = temp;
                }
            }
        }

        public static void maskBits8(int bitsStored, int numElements, ref byte[] array, int startIndex)
        {
            int shift = 8 - bitsStored;
            for (int i = 0; i < numElements; i++)
            {
                array[startIndex + i] = (byte)((array[startIndex + i] << shift) >> shift);
            }
        }

        public static void maskBits16(int bitsStored, int numElements, ref byte[] array, int startIndex)
        {
            int shift0 =             16 - bitsStored;
            int shift1 = Mathf.Max(0, 8 - bitsStored);
            for (int i = 0; i < numElements; i++)
            {
                array[startIndex * 2 + i * 2]       = (byte)((array[startIndex * 2 + i * 2]     << shift0) >> shift0);
                array[startIndex * 2 + i * 2 + 1]   = (byte)((array[startIndex * 2 + i * 2 + 1] << shift1) >> shift1);
            }
        }

        public static void maskBits32(int bitsStored, int numElements, ref byte[] array, int startIndex)
        {
            int shift0 =              32 - bitsStored;
            int shift1 = Mathf.Max(0, 24 - bitsStored);
            int shift2 = Mathf.Max(0, 16 - bitsStored);
            int shift3 = Mathf.Max(0,  8 - bitsStored);
            for (int i = 0; i < numElements; i++)
            {
                array[startIndex * 4 + i * 4]       = (byte)((array[startIndex * 4 + i * 4]     << shift0) >> shift0);
                array[startIndex * 4 + i * 4 + 1]   = (byte)((array[startIndex * 4 + i * 4 + 1] << shift1) >> shift1);
                array[startIndex * 4 + i * 4 + 2]   = (byte)((array[startIndex * 4 + i * 4 + 2] << shift2) >> shift2);
                array[startIndex * 4 + i * 4 + 3]   = (byte)((array[startIndex * 4 + i * 4 + 3] << shift3) >> shift3);
            }
        }

        static string getElemValueAsString(byte[] arr, int index, string vr, int length, bool needsSwapping)
        {
            string returnStr = "";
            switch (vr)
            {
                case "AT":  //Attribute Tag 4 bytes fixed
                    returnStr = BitConverter.ToUInt16(arr, index).ToString() + "\\" + BitConverter.ToUInt16(arr, index + 2).ToString();
                    break;
                case "OB":  //Other byte
                    for (int i = 0; i < length - 1; i++)
                    {
                        returnStr += arr[index + i].ToString() + "\\";
                    }
                    returnStr += arr[index + length - 1].ToString();
                    break;
                case "FD":  //Double
                case "OD":  //Other double
                    if(needsSwapping)
                    {
                        swapBytes(length / 8, 8, ref arr, index);
                    }
                    for (int i = 0; i < length - 8; i += 8)
                    {
                        returnStr += BitConverter.ToDouble(arr, index + i).ToString() + "\\";
                    }
                    returnStr += BitConverter.ToDouble(arr, index + length - 8).ToString();
                    break;
                case "FL":  //Float
                case "OF":  //Other float
                    if (needsSwapping)
                    {
                        swapBytes(length / 4, 4, ref arr, index);
                    }
                    for (int i = 0; i < length - 4; i += 4)
                    {
                        returnStr += BitConverter.ToSingle(arr, index + i).ToString() + "\\";
                    }
                    returnStr += BitConverter.ToSingle(arr, index + length - 4).ToString();
                    break;
                case "SL":  //Signed long
                case "OL":  //Other long
                    if (needsSwapping)
                    {
                        swapBytes(length / 4, 4, ref arr, index);
                    }
                    for (int i = 0; i < length - 4; i += 4)
                    {
                        returnStr += BitConverter.ToInt32(arr, index + i).ToString() + "\\";
                    }
                    returnStr += BitConverter.ToInt32(arr, index + length - 4).ToString();
                    break;
                case "SS":  //Signed short
                case "OW":  //Other word
                    if (needsSwapping)
                    {
                        swapBytes(length / 2, 2, ref arr, index);
                    }
                    for (int i = 0; i < length - 2; i += 2)
                    {
                        returnStr += BitConverter.ToInt16(arr, index + i).ToString() + "\\";
                    }
                    returnStr += BitConverter.ToInt16(arr, index + length - 2).ToString();
                    break;
                case "UL":  //Unsigned long
                    if (needsSwapping)
                    {
                        swapBytes(length / 4, 4, ref arr, index);
                    }
                    for (int i = 0; i < length - 4; i += 4)
                    {
                        returnStr += BitConverter.ToUInt32(arr, index + i).ToString() + "\\";
                    }
                    returnStr += BitConverter.ToUInt32(arr, index + length - 4).ToString();
                    break;
                case "US":  //Unsigned short
                    if (needsSwapping)
                    {
                        swapBytes(length / 2, 2, ref arr, index);
                    }
                    for (int i = 0; i < length - 2; i += 2)
                    {
                        returnStr += BitConverter.ToUInt16(arr, index + i).ToString() + "\\";
                    }
                    returnStr += BitConverter.ToUInt16(arr, index + length - 2).ToString();
                    break;
                case "AE": //Application Entity         16  bytes max
                case "AS": //Age String                 4   bytes fixed
                case "CS": //Code String                16  bytes max
                case "DA": //Date                       8   bytes fixed
                case "DS": //Decimal String             16  bytes max
                case "DT": //Date Time                  26  bytes max
                case "IS": //Integer String             12  bytes max
                case "LO": //Long String                64  bytes max
                case "LT": //Long Text                  10240  bytes max
                case "PN": //Patient Name               64  bytes max
                case "SH": //Short String               16  bytes max
                case "ST": //Short Text                 1024  bytes max
                case "TM": //Time                       14  bytes max
                case "UC": //Unlimited chars            
                case "UI": //Unique Identifier          64  bytes max
                case "UR": //Universal Resource Identifier (URL, URI)
                case "UT": //Unlimited Text
                    returnStr = Encoding.ASCII.GetString(arr, index, (int)length);
                    break;
                case "SQ":
                case "UN":
                default:
                    break;
            }
            return returnStr;
        }

        public static Dictionary<string, string> getElementsOfFile(string fName, bool needsSwapping)
        {
            if (!File.Exists(fName))
            {
                return null;
            }
            Int64 fileSize = new System.IO.FileInfo(fName).Length;
            int numBytes2Read = 16 * 1024 * 1024;
            if (fileSize < numBytes2Read)
            {
                numBytes2Read = (int)(fileSize);
            }
            BinaryReader reader;
            byte[] val = null;
            try
            {
                reader = new BinaryReader(File.Open(fName, FileMode.Open));
                if (reader != null)
                {
                    val = reader.ReadBytes(numBytes2Read);
                }
            }
            catch (IOException)                 {   Debug.Log("IOException: " + fName);                     return null; }
            catch (UnauthorizedAccessException) {   Debug.Log("UnauthorizedAccessException: " + fName);     return null; }
            catch (NotSupportedException)       {   Debug.Log("NotSupportedException: " + fName);           return null; }
            catch (ArgumentException)           {   Debug.Log("ArgumentException: " + fName);               return null; }
            if (val.Length < numBytes2Read)
            {
                return null;
            }
            if (( val[128] != 'D' ||
                  val[129] != 'I' ||
                  val[130] != 'C' ||
                  val[131] != 'M'))
            {
                return null;
            }
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("FilePath", Path.GetFullPath(fName));
            tags.Add("FileName", Path.GetFileName(fName));
            //UInt16 group, element;
            UInt32 tag;
            int byteIndex = 132;
            while (byteIndex < numBytes2Read)
            {
                if (needsSwapping)
                {
                    swapBytes(2, 2, ref val, byteIndex);
                }
                //group = BitConverter.ToUInt16(val, byteIndex);
                //element = BitConverter.ToUInt16(val, byteIndex + 2);
                tag = BitConverter.ToUInt32(val, byteIndex);
                byteIndex += 4;
                string vr = Encoding.ASCII.GetString(val, byteIndex, 2);
                byteIndex += 2;
                long length;
                if (   vr.Equals("AE") || vr.Equals("AS") || vr.Equals("AT")
                    || vr.Equals("CS") || vr.Equals("DA") || vr.Equals("DS")
                    || vr.Equals("DT") || vr.Equals("FL") || vr.Equals("FD")
                    || vr.Equals("IS") || vr.Equals("LO") || vr.Equals("LT")
                    || vr.Equals("PN") || vr.Equals("SH") || vr.Equals("SL")
                    || vr.Equals("SS") || vr.Equals("ST") || vr.Equals("TM")
                    || vr.Equals("UI") || vr.Equals("UL") || vr.Equals("US"))
                {
                    if (needsSwapping)
                    {
                        swapBytes(1, 2, ref val, byteIndex);
                    }
                    length = BitConverter.ToUInt16(val, byteIndex);
                    byteIndex += 2;
                }
                else if (  vr.Equals("VR") || vr.Equals("OB") || vr.Equals("OD")
                        || vr.Equals("OF") || vr.Equals("OL") || vr.Equals("OW")
                        || vr.Equals("SQ") || vr.Equals("UC") || vr.Equals("UR")
                        || vr.Equals("UT") || vr.Equals("UN"))
                {
                    byteIndex += 2;
                    if (needsSwapping)
                    {
                        swapBytes(1, 4, ref val, byteIndex);
                    }
                    length = BitConverter.ToUInt32(val, byteIndex);
                    byteIndex += 4;
                }
                else
                {
                    vr = "IM";
                    byteIndex -= 2;
                    if (needsSwapping)
                    {
                        swapBytes(1, 4, ref val, byteIndex);
                    }
                    length = BitConverter.ToUInt32(val, byteIndex);
                    byteIndex += 4;
                }
                if (length == 0)
                {
                    byteIndex += (int)length;
                    continue;
                }
                string dictKey;
                if (dicomDict.ContainsKey(tag))
                {
                    dictKey = dicomDict[tag];
                }
                else
                {
                    byteIndex += (int)length;
                    continue;
                }
                if (vr.Equals("IM"))
                {
                    vr = dictKey.Substring(0, 2);
                }
                dictKey = dictKey.Substring(2);

                if (vr.Equals("SQ") || vr.Equals("UN"))
                {
                    if (length == 0xffffffff)
                        length = 8;
                    byteIndex += (int)length;
                    continue;
                }
                if (dictKey.Equals("PixelData"))
                {
                    tags.Add("PixelDataAt", byteIndex.ToString() + "\\" + length.ToString());
                    break;
                }
                if (dictKey.Equals("FloatPixelData"))
                {
                    tags.Add("FloatPixelDataAt", byteIndex.ToString() + "\\" + length.ToString());
                    break;
                }
                if (dictKey.Equals("DoubleFloatPixelData"))
                {
                    tags.Add("DoubleFloatPixelDataAt", byteIndex.ToString() + "\\" + length.ToString());
                    break;
                }
                string dictValue = getElemValueAsString(val, byteIndex, vr, (int)length, needsSwapping);
                if (!tags.ContainsKey(dictKey))
                    tags.Add(dictKey, dictValue);
                byteIndex += (int)length;
            }
            reader.BaseStream.Close();
            reader.Close();
            return tags;
        }

        public static string tag2String(UInt16 group, UInt16 element)
        {
            string gString = group.ToString("X");
            int gStringLength = gString.Length;
            string eString = element.ToString("X");
            int eStringLength = eString.Length;
            for (int i = 0; i < 4 - gStringLength; i++)
            {
                gString = gString.Insert(0, "0");
            }
            for (int i = 0; i < 4 - eStringLength; i++)
            {
                eString = eString.Insert(0, "0");
            }
            return "(" + gString + "," + eString + ")";
        }

        public class DictStrStrComparer : IComparer<Dictionary<string, string>>
        {
            public string sortKey { get; set; }
            public DictStrStrComparer(string iSortKey)
            {
                sortKey = iSortKey;
            }
            public int Compare(Dictionary<string, string> d1, Dictionary<string, string> d2)
            {
                float f1, f2;
                bool b1 = float.TryParse(d1[sortKey], NumberStyles.Any, CultureInfo.InvariantCulture, out f1);
                bool b2 = float.TryParse(d2[sortKey], NumberStyles.Any, CultureInfo.InvariantCulture, out f2);
                if (b1 && b2)
                {
                    if (f1 > f2)
                    {
                        return 1;
                    }
                    if (f1 < f2)
                    {
                        return -1;
                    }
                    if (f1 == f2)
                    {
                        return 0;
                    }
                }
                if (b1 && !b2)
                {
                    return -1;
                }
                if (!b1 && b2)
                {
                    return 1;
                }
                return string.Compare(d1[sortKey], d2[sortKey], true);
            }
        }

        public class DcmInfo
        {
            public int nx;
            public int ny;
            public int nz;
            public float dx;
            public float dy;
            public float dz;
            public int numVoxels;
            public int bytesPerVoxel;
            public int bytesPerChannel;
            public int numChannels;
            public int bitsStored;
            public int pixelRepresentation;
            public int pixelDataLength;
            public PhotometricInterpretationCode photometricInterpretation;
            public PlanarConfiguration planarConfiguration;
            public Dictionary<string, string> tags;
            public FormatCode format;

            public int getDcmInfo(string fName, bool needsSwapping)
            {
                tags = getElementsOfFile(fName, needsSwapping);
                if (tags == null)
                {
                    return 0;
                }
                string[] pixDataLocLen;
                if (tags.ContainsKey("PixelDataAt"))
                {
                    pixDataLocLen = tags["PixelDataAt"].Split('\\');
                }
                else if (tags.ContainsKey("FloatPixelDataAt"))
                {
                    pixDataLocLen = tags["FloatPixelDataAt"].Split('\\');
                    Debug.Log("Error: FloatPixelDataAt not supported!");
                    return 0;
                }
                else if (tags.ContainsKey("DoubleFloatPixelDataAt"))
                {
                    pixDataLocLen = tags["DoubleFloatPixelDataAt"].Split('\\');
                    Debug.Log("Error: DoubleFloatPixelData not supported!");
                    return 0;
                }
                else
                {
                    Debug.Log("Error: No PixelData!");
                    return 0;
                }
                //int pixelDataIndex = int.Parse(pixDataLocLen[0], CultureInfo.InvariantCulture);
                pixelDataLength = int.Parse(pixDataLocLen[1], CultureInfo.InvariantCulture);
                numChannels = 1;
                if (tags.ContainsKey("SamplesPerPixel"))
                {
                    numChannels = int.Parse(tags["SamplesPerPixel"], CultureInfo.InvariantCulture);
                }
                if (!tags.ContainsKey("BitsAllocated"))
                {
                    Debug.Log("Error: No BitsAllocated element present!");
                    return 0;
                }
                bitsStored = int.Parse(tags["BitsAllocated"], CultureInfo.InvariantCulture);
                bytesPerChannel = (int)Math.Ceiling( bitsStored / 8.0);
                bytesPerVoxel = bytesPerChannel * numChannels;
                if (tags.ContainsKey("BitsStored"))
                {
                    bitsStored = int.Parse(tags["BitsStored"], CultureInfo.InvariantCulture);
                }
                pixelRepresentation = 0;
                if (tags.ContainsKey("PixelRepresentation"))
                {
                    pixelRepresentation = int.Parse(tags["PixelRepresentation"], CultureInfo.InvariantCulture);
                }
                planarConfiguration = PlanarConfiguration.Interlaced;
                if (tags.ContainsKey("PlanarConfiguration"))
                {
                    planarConfiguration = (PlanarConfiguration)  (1 + int.Parse(tags["PlanarConfiguration"], CultureInfo.InvariantCulture));
                }
                if (numChannels == 3)
                {
                    photometricInterpretation = PhotometricInterpretationCode.RGB;
                }
                else
                {
                    photometricInterpretation = PhotometricInterpretationCode.MONOCHROME2;
                }
                if (tags.ContainsKey("PhotometricInterpretation"))
                {
                    if(tags["PhotometricInterpretation"].Equals("MONOCHROME1"))
                    {
                        photometricInterpretation = PhotometricInterpretationCode.MONOCHROME1;
                    }
                    else if (tags["PhotometricInterpretation"].Equals("MONOCHROME2"))
                    {
                        photometricInterpretation = PhotometricInterpretationCode.MONOCHROME2;
                    }
                    else if (tags["PhotometricInterpretation"].Equals("RGB"))
                    {
                        photometricInterpretation = PhotometricInterpretationCode.RGB;
                    }
                }else
                {
                    Debug.Log("No PhotometricInterpretation tag present. Assuming " + photometricInterpretation);
                }
                numVoxels = pixelDataLength / bytesPerVoxel;
                if (!tags.ContainsKey("Columns"))
                {
                    Debug.Log("Error: No Columns element present!");
                    return 0;
                }
                nx = int.Parse(tags["Columns"], CultureInfo.InvariantCulture);
                if (!tags.ContainsKey("Rows"))
                {
                    Debug.Log("Error: No Rows element present!");
                    return 0;
                }
                ny = int.Parse(tags["Rows"], CultureInfo.InvariantCulture);
                nz = numVoxels / nx / ny;
                if (nz < 1)
                {
                    Debug.Log("Error: Not enough PixelData for specified dimensions.");
                    return 0;
                }
                dx = 1;
                dy = 1;
                dz = 1;
                if (tags.ContainsKey("PixelSpacing"))
                {
                    string[] dxdy = tags["PixelSpacing"].Split('\\');
                    dx = float.Parse(dxdy[0], CultureInfo.InvariantCulture);
                    dy = float.Parse(dxdy[1], CultureInfo.InvariantCulture);
                }
                if (tags.ContainsKey("SpacingBetweenSlices"))
                {
                    dz = float.Parse(tags["SpacingBetweenSlices"], CultureInfo.InvariantCulture);
                }
                else if (tags.ContainsKey("SliceThickness"))
                {
                    dz = float.Parse(tags["SliceThickness"], CultureInfo.InvariantCulture);
                }
                format = FormatCode.DICOM_TYPE_UNKNOWN;
                if(numChannels == 4 && bytesPerVoxel == 4)
                {
                    format = FormatCode.DICOM_TYPE_RGBA32;
                }
                else if (numChannels == 3 && bytesPerVoxel == 3)
                {
                    format = FormatCode.DICOM_TYPE_RGB24;
                }
                else if (numChannels == 1)
                {
                    if(bytesPerVoxel == 1)
                    {
                        format = pixelRepresentation == 0 ? FormatCode.DICOM_TYPE_UINT8 : FormatCode.DICOM_TYPE_INT8;
                    }
                    else if(bytesPerVoxel==2)
                    {
                        format = pixelRepresentation == 0 ? FormatCode.DICOM_TYPE_UINT16 : FormatCode.DICOM_TYPE_INT16;
                    }
                    else if(bytesPerVoxel==4)
                    {
                        if (tags.ContainsKey("PixelDataAt"))
                        {
                            format = pixelRepresentation == 0 ? FormatCode.DICOM_TYPE_UINT32 : FormatCode.DICOM_TYPE_INT32;
                        }
                        else if (tags.ContainsKey("FloatPixelDataAt"))
                        {
                            format = FormatCode.DICOM_TYPE_FLOAT32;
                        }
                    }
                }
                if(format == FormatCode.DICOM_TYPE_UNKNOWN)
                {
                    Debug.Log("Error: Unsupported DICOM format.");
                    return 0;
                }
                return 1;
            }
        }
    }
    
    public class DCM2Volume
    {
        public Volume volume;
        public DICOM.DcmInfo dcmInfo;

        public IEnumerator loadFile(string fName, VolumeTextureFormat forceTextureFormat, FloatEvent loadingProgressChanged, PlanarConfiguration planarConfig, Ref<bool> completed, Ref<int> returned)
        {
            returned.val = 1;
            fName = Path.GetFullPath(fName);
            if (!File.Exists(fName))
            {
                Debug.Log("File doesn't exist: " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            dcmInfo = new DICOM.DcmInfo();
            bool needsSwapping = false;
            if (DICOM.getByteOrder() == DICOM.MSB_FIRST)
            {
                needsSwapping = true;
            }
            if (dcmInfo.getDcmInfo(fName, needsSwapping) == 0)
            {
                Debug.Log("Unable to obtain DICOM info for " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            if (!dcmInfo.tags.ContainsKey("TransferSyntaxUID"))
            {
                Debug.Log("No TransferSyntax specified in " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }                                                                                                      
            if (!dcmInfo.tags["TransferSyntaxUID"].Equals("1.2.840.10008.1.2\0") && !dcmInfo.tags["TransferSyntaxUID"].Equals("1.2.840.10008.1.2.1\0"))
            {
                Debug.Log("Unsupported TransferSyntax " + dcmInfo.tags["TransferSyntaxUID"].Substring(0, dcmInfo.tags["TransferSyntaxUID"].Length - 1) + " in " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            List <Dictionary<string, string>> tagList = null;
            bool multipleFiles = false;
            if (dcmInfo.nz == 1)
            {
                multipleFiles = true;
                int selectBy = -1;
                string[] selectionStrings = { "SeriesInstanceUID", "SeriesNumber", "SeriesDescription" };
                int sortBy = -1;
                string[] sortStrings = { "InstanceNumber", "MediaStorageSOPInstanceUID", "SOPInstanceUID", "SliceLocation", "FileName" };
                tagList = new List<Dictionary<string, string>>();
                if      (dcmInfo.tags.ContainsKey(selectionStrings[0])) {   selectBy = 0;   }
                else if (dcmInfo.tags.ContainsKey(selectionStrings[1])) {   selectBy = 1;   }
                else if (dcmInfo.tags.ContainsKey(selectionStrings[2])) {   selectBy = 2;   }
                if      (dcmInfo.tags.ContainsKey(sortStrings[0]))      {   sortBy = 0;     }
                else if (dcmInfo.tags.ContainsKey(sortStrings[1]))      {   sortBy = 1;     }
                else if (dcmInfo.tags.ContainsKey(sortStrings[2]))      {   sortBy = 2;     }
                else if (dcmInfo.tags.ContainsKey(sortStrings[3]))      {   sortBy = 3;     }
                else if (dcmInfo.tags.ContainsKey(sortStrings[4]))      {   sortBy = 4;     }
                tagList.Add(dcmInfo.tags);
                string dir = Path.GetDirectoryName(fName);
                string[] filePaths = Directory.GetFiles(dir);
                foreach (string filePath in filePaths)
                {
                    if (Path.GetFileName(filePath).Equals(Path.GetFileName(fName)))
                    {
                        continue;
                    }
                    var tagsOfFile = DICOM.getElementsOfFile(filePath, needsSwapping);
                    if (tagsOfFile == null)
                    {
                        continue;
                    }
                    if (selectBy >= 0 && !tagsOfFile[selectionStrings[selectBy]].Equals(dcmInfo.tags[selectionStrings[selectBy]]))
                    {
                        continue;
                    }
                    if (dcmInfo.pixelDataLength != int.Parse(tagsOfFile["PixelDataAt"].Split('\\')[1], CultureInfo.InvariantCulture))
                    {
                        continue;
                    }
                    tagList.Add(tagsOfFile);
                }
                dcmInfo.nz = tagList.Count;
                tagList.Sort(new DICOM.DictStrStrComparer(sortStrings[sortBy]));
            }
            int numPixels = dcmInfo.nx * dcmInfo.ny;
            volume = new Volume();
            volume.nx = Mathf.NextPowerOfTwo(dcmInfo.nx);
            volume.ny = Mathf.NextPowerOfTwo(dcmInfo.ny);
            volume.nz = Mathf.NextPowerOfTwo(dcmInfo.nz);
            volume.dx = dcmInfo.dx;
            volume.dy = dcmInfo.dy;
            volume.dz = dcmInfo.dz;
            int startX = (int)Mathf.Floor((volume.nx - dcmInfo.nx) / 2.0f);
            int startY = (int)Mathf.Floor((volume.ny - dcmInfo.ny) / 2.0f);
            int startZ = (int)Mathf.Floor((volume.nz - dcmInfo.nz) / 2.0f);
            volume.numVoxels = volume.nx * volume.ny * volume.nz;
            Color[] volColors = null;
            Color32[] volColors32 = null;
            float progressMultiplier = 1;
            switch (dcmInfo.format)
            {
                case DICOM.FormatCode.DICOM_TYPE_INT8:
                case DICOM.FormatCode.DICOM_TYPE_UINT8:
                    volume.format = TextureFormat.Alpha8;
                    volColors32 = new Color32[volume.numVoxels];
                    break;
                case DICOM.FormatCode.DICOM_TYPE_INT16:
                case DICOM.FormatCode.DICOM_TYPE_UINT16:
                    volume.format = TextureFormat.RHalf;
                    volColors = new Color[volume.numVoxels];
                    progressMultiplier = 0.5f;
                    break;
                case DICOM.FormatCode.DICOM_TYPE_INT32:
                case DICOM.FormatCode.DICOM_TYPE_UINT32:
                case DICOM.FormatCode.DICOM_TYPE_FLOAT32:
                    volume.format = TextureFormat.RFloat;
                    volColors = new Color[volume.numVoxels];
                    progressMultiplier = 0.5f;
                    break;
                case DICOM.FormatCode.DICOM_TYPE_RGB24:
                    volume.format = TextureFormat.RGB24;
                    volColors32 = new Color32[volume.numVoxels];
                    break;
                case DICOM.FormatCode.DICOM_TYPE_RGBA32:
                    volume.format = TextureFormat.RGBA32;
                    volColors32 = new Color32[volume.numVoxels];
                    break;
                default:
                    Debug.Log("Format not supported: " + dcmInfo.format);
                    returned.val = 0;
                    completed.val = true;
                    yield break;
            }
            switch (forceTextureFormat)
            {
                case VolumeTextureFormat.Alpha8:
                    volume.format = TextureFormat.Alpha8;
                    break;
                case VolumeTextureFormat.RFloat:
                    volume.format = TextureFormat.RFloat;
                    break;
                case VolumeTextureFormat.RHalf:
                    volume.format = TextureFormat.RHalf;
                    break;
                case VolumeTextureFormat.RGB24:
                    volume.format = TextureFormat.RGB24;
                    break;
                case VolumeTextureFormat.RGBA32:
                    volume.format = TextureFormat.RGBA32;
                    break;
                case VolumeTextureFormat.ARGB32:
                    volume.format = TextureFormat.ARGB32;
                    break;
                case VolumeTextureFormat.RGBAFloat:
                    volume.format = TextureFormat.RGBAFloat;
                    break;
                case VolumeTextureFormat.RGBAHalf:
                    volume.format = TextureFormat.RGBAHalf;
                    break;
            }
            if(planarConfig != PlanarConfiguration.Native)
            {
                dcmInfo.planarConfiguration = planarConfig;
            }
            try
            {
                volume.texture = new Texture3D(volume.nx, volume.ny, volume.nz, volume.format, false);
            }
            catch (UnityException)
            {
                Debug.Log("Couldn't create Texture3D(" + volume.nx + ", " + volume.ny + ", " + volume.nz + ", " + volume.format + ", " + "false).\nTry forcing a different format in the VolumeFileLoader component.");
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            if (volume.texture == null)
            {
                Debug.Log("Couldn't create Texture3D(" + volume.nx + ", " + volume.ny + ", " + volume.nz + ", " + volume.format + ", " + "false).\nTry forcing a different format in the VolumeFileLoader component.");
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            BinaryReader imgReader = null;
            string[] pixDataLocLen = null;
            int pixelDataIndex = 0;
            //int pixelDataLength = 0;
            if (!multipleFiles)
            {
                //Already checked for access issues a few ms ago.
                imgReader = new BinaryReader(File.Open(fName, FileMode.Open));
                pixDataLocLen = dcmInfo.tags["PixelDataAt"].Split('\\');
                pixelDataIndex = int.Parse(pixDataLocLen[0], CultureInfo.InvariantCulture);
                //pixelDataLength = int.Parse(pixDataLocLen[1], CultureInfo.InvariantCulture);
                imgReader.BaseStream.Seek(pixelDataIndex, SeekOrigin.Begin);
            }
            int numBytesPerSlice = dcmInfo.bytesPerVoxel * numPixels;
            byte[] slice = new byte[numBytesPerSlice];
            int stopZ = startZ + dcmInfo.nz;
            int stopY = startY + dcmInfo.ny;
            int stopX = startX + dcmInfo.nx;
            float maxValue = 0;
            float minValue = Mathf.Infinity;
            int cumZ, cumY, zi, index;
            for (int z = startZ; z < stopZ; z++)
            {
                zi = z - startZ;
                cumZ = z * volume.ny * volume.nx;
                if(multipleFiles)
                {
                    imgReader = new BinaryReader(File.Open(tagList[zi]["FilePath"], FileMode.Open));
                    pixDataLocLen = tagList[zi]["PixelDataAt"].Split('\\');
                    pixelDataIndex = int.Parse(pixDataLocLen[0], CultureInfo.InvariantCulture);
                    //pixelDataLength = int.Parse(pixDataLocLen[1], CultureInfo.InvariantCulture);
                    imgReader.BaseStream.Seek(pixelDataIndex, SeekOrigin.Begin);
                }
                slice = imgReader.ReadBytes(numBytesPerSlice);
                if (needsSwapping && dcmInfo.bytesPerChannel > 1)
                {
                    DICOM.swapBytes(numBytesPerSlice / dcmInfo.bytesPerChannel, dcmInfo.bytesPerChannel, ref slice, 0);
                }
                if(dcmInfo.photometricInterpretation == DICOM.PhotometricInterpretationCode.MONOCHROME1)
                {
                    for(int i=0; i < numBytesPerSlice; i++)
                    {
                        slice[i] = (byte)~slice[i];
                    }
                }
                int byteIndex = 0;
                switch (dcmInfo.format)
                {
                    case DICOM.FormatCode.DICOM_TYPE_INT8:
                        if (dcmInfo.bitsStored != 8) { DICOM.maskBits8(dcmInfo.bitsStored, numBytesPerSlice, ref slice, 0); }
                        byte valueByte;
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                valueByte = slice[byteIndex] > 127 ? (byte)(slice[byteIndex] - 128) : (byte)(slice[byteIndex] + 128);
                                volColors32[cumZ + cumY + x] = new Color32(valueByte, 0, 0, valueByte);
                                byteIndex++;
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_UINT8:
                        if (dcmInfo.bitsStored != 8) { DICOM.maskBits8(dcmInfo.bitsStored, numBytesPerSlice, ref slice, 0); }
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex], 0, 0, slice[byteIndex]);
                                byteIndex++;
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_INT16:
                        if (dcmInfo.bitsStored != 16) { DICOM.maskBits16(dcmInfo.bitsStored, numPixels, ref slice, 0); }
                        UInt16 valueUInt16;
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                valueUInt16 = BitConverter.ToUInt16(slice, byteIndex);
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(valueUInt16 > 32767 ? (valueUInt16 - 32768) : (valueUInt16 + 32768), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 2;
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_UINT16:
                        if (dcmInfo.bitsStored != 16) { DICOM.maskBits16(dcmInfo.bitsStored, numPixels, ref slice, 0); }
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToUInt16(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 2;
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_INT32:
                        if (dcmInfo.bitsStored != 32) { DICOM.maskBits32(dcmInfo.bitsStored, numPixels, ref slice, 0); }
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToInt32(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 4;
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_UINT32:
                        if (dcmInfo.bitsStored != 32) { DICOM.maskBits32(dcmInfo.bitsStored, numPixels, ref slice, 0); }
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToUInt32(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 4;
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_FLOAT32:
                        if (dcmInfo.bitsStored != 32) { DICOM.maskBits32(dcmInfo.bitsStored, numPixels, ref slice, 0); }
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index] = new Color(BitConverter.ToSingle(slice, byteIndex), 0, 0, 0);
                                if (maxValue < volColors[index].r)
                                {
                                    maxValue = volColors[index].r;
                                }
                                if (minValue > volColors[index].r)
                                {
                                    minValue = volColors[index].r;
                                }
                                byteIndex += 4;
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_RGB24:
                        if (dcmInfo.bitsStored != 8) { DICOM.maskBits8(dcmInfo.bitsStored, numBytesPerSlice, ref slice, 0); }
                        if (dcmInfo.planarConfiguration == PlanarConfiguration.Separated)
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], 0, 0, 255);
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].g = slice[byteIndex++];
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].b = slice[byteIndex++];
                                }
                            }
                        }
                        else
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], slice[byteIndex++], slice[byteIndex++], 255);
                                }
                            }
                        }
                        break;
                    case DICOM.FormatCode.DICOM_TYPE_RGBA32:
                        if (dcmInfo.bitsStored != 8) { DICOM.maskBits8(dcmInfo.bitsStored, numBytesPerSlice, ref slice, 0); }
                        if (dcmInfo.planarConfiguration == PlanarConfiguration.Separated)
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], 0, 0, 0);
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].g = slice[byteIndex++];
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].b = slice[byteIndex++];
                                }
                            }
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x].a = slice[byteIndex++];
                                }
                            }
                        }
                        else
                        {
                            for (int y = startY; y < stopY; y++)
                            {
                                cumY = y * volume.nx;
                                for (int x = startX; x < stopX; x++)
                                {
                                    volColors32[cumZ + cumY + x] = new Color32(slice[byteIndex++], slice[byteIndex++], slice[byteIndex++], slice[byteIndex++]);
                                }
                            }
                        }
                        break;
                }
                if (multipleFiles)
                {
                    imgReader.BaseStream.Close();
                    imgReader.Close();
                }
                loadingProgressChanged.Invoke(progressMultiplier * (z - startZ + 1) / dcmInfo.nz);
                yield return null;
            }
            if (!multipleFiles)
            {
                imgReader.BaseStream.Close();
                imgReader.Close();
            }
            switch (dcmInfo.format)
            {
                case DICOM.FormatCode.DICOM_TYPE_INT8:
                case DICOM.FormatCode.DICOM_TYPE_UINT8:
                case DICOM.FormatCode.DICOM_TYPE_RGB24:
                case DICOM.FormatCode.DICOM_TYPE_RGBA32:
                    volume.texture.SetPixels32(volColors32);
                    break;
                case DICOM.FormatCode.DICOM_TYPE_INT16:
                case DICOM.FormatCode.DICOM_TYPE_UINT16:
                case DICOM.FormatCode.DICOM_TYPE_INT32:
                case DICOM.FormatCode.DICOM_TYPE_UINT32:
                case DICOM.FormatCode.DICOM_TYPE_FLOAT32:
                    float valueRange = maxValue - minValue;
                    for (int z = startZ; z < stopZ; z++)
                    {
                        cumZ = z * volume.ny * volume.nx;
                        for (int y = startY; y < stopY; y++)
                        {
                            cumY = y * volume.nx;
                            for (int x = startX; x < stopX; x++)
                            {
                                index = cumZ + cumY + x;
                                volColors[index].r = (volColors[index].r - minValue) / valueRange;
                                volColors[index].a = volColors[index].r;
                            }
                        }
                        loadingProgressChanged.Invoke(0.5f + progressMultiplier * (z - startZ + 1) / dcmInfo.nz);
                        yield return null;
                    }
                    volume.texture.SetPixels(volColors);
                    break;
            }
            volume.texture.filterMode = FilterMode.Trilinear;
            volume.texture.wrapMode = TextureWrapMode.Clamp;
            volume.texture.Apply();

            completed.val = true;
        }
    }
}
#endif