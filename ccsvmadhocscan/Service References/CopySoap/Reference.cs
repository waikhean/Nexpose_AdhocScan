﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ccsvmadhocscan.CopySoap {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", ConfigurationName="CopySoap.CopySoap")]
    public interface CopySoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://schemas.microsoft.com/sharepoint/soap/CopyIntoItemsLocal", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalResponse CopyIntoItemsLocal(global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://schemas.microsoft.com/sharepoint/soap/CopyIntoItemsLocal", ReplyAction="*")]
        System.Threading.Tasks.Task<global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalResponse> CopyIntoItemsLocalAsync(global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://schemas.microsoft.com/sharepoint/soap/CopyIntoItems", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        global::ccsvmadhocscan.CopySoap.CopyIntoItemsResponse CopyIntoItems(global::ccsvmadhocscan.CopySoap.CopyIntoItemsRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://schemas.microsoft.com/sharepoint/soap/CopyIntoItems", ReplyAction="*")]
        System.Threading.Tasks.Task<global::ccsvmadhocscan.CopySoap.CopyIntoItemsResponse> CopyIntoItemsAsync(global::ccsvmadhocscan.CopySoap.CopyIntoItemsRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://schemas.microsoft.com/sharepoint/soap/GetItem", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        global::ccsvmadhocscan.CopySoap.GetItemResponse GetItem(global::ccsvmadhocscan.CopySoap.GetItemRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://schemas.microsoft.com/sharepoint/soap/GetItem", ReplyAction="*")]
        System.Threading.Tasks.Task<global::ccsvmadhocscan.CopySoap.GetItemResponse> GetItemAsync(global::ccsvmadhocscan.CopySoap.GetItemRequest request);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/")]
    public partial class CopyResult : object, System.ComponentModel.INotifyPropertyChanged {
        
        private CopyErrorCode errorCodeField;
        
        private string errorMessageField;
        
        private string destinationUrlField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public CopyErrorCode ErrorCode {
            get {
                return this.errorCodeField;
            }
            set {
                this.errorCodeField = value;
                this.RaisePropertyChanged("ErrorCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ErrorMessage {
            get {
                return this.errorMessageField;
            }
            set {
                this.errorMessageField = value;
                this.RaisePropertyChanged("ErrorMessage");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DestinationUrl {
            get {
                return this.destinationUrlField;
            }
            set {
                this.destinationUrlField = value;
                this.RaisePropertyChanged("DestinationUrl");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/")]
    public enum CopyErrorCode {
        
        /// <remarks/>
        Success,
        
        /// <remarks/>
        DestinationInvalid,
        
        /// <remarks/>
        DestinationMWS,
        
        /// <remarks/>
        SourceInvalid,
        
        /// <remarks/>
        DestinationCheckedOut,
        
        /// <remarks/>
        InvalidUrl,
        
        /// <remarks/>
        Unknown,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/")]
    public partial class FieldInformation : object, System.ComponentModel.INotifyPropertyChanged {
        
        private FieldType typeField;
        
        private string displayNameField;
        
        private string internalNameField;
        
        private System.Guid idField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FieldType Type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
                this.RaisePropertyChanged("Type");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DisplayName {
            get {
                return this.displayNameField;
            }
            set {
                this.displayNameField = value;
                this.RaisePropertyChanged("DisplayName");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InternalName {
            get {
                return this.internalNameField;
            }
            set {
                this.internalNameField = value;
                this.RaisePropertyChanged("InternalName");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.Guid Id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
                this.RaisePropertyChanged("Id");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
                this.RaisePropertyChanged("Value");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/")]
    public enum FieldType {
        
        /// <remarks/>
        Invalid,
        
        /// <remarks/>
        Integer,
        
        /// <remarks/>
        Text,
        
        /// <remarks/>
        Note,
        
        /// <remarks/>
        DateTime,
        
        /// <remarks/>
        Counter,
        
        /// <remarks/>
        Choice,
        
        /// <remarks/>
        Lookup,
        
        /// <remarks/>
        Boolean,
        
        /// <remarks/>
        Number,
        
        /// <remarks/>
        Currency,
        
        /// <remarks/>
        URL,
        
        /// <remarks/>
        Computed,
        
        /// <remarks/>
        Threading,
        
        /// <remarks/>
        Guid,
        
        /// <remarks/>
        MultiChoice,
        
        /// <remarks/>
        GridChoice,
        
        /// <remarks/>
        Calculated,
        
        /// <remarks/>
        File,
        
        /// <remarks/>
        Attachments,
        
        /// <remarks/>
        User,
        
        /// <remarks/>
        Recurrence,
        
        /// <remarks/>
        CrossProjectLink,
        
        /// <remarks/>
        ModStat,
        
        /// <remarks/>
        AllDayEvent,
        
        /// <remarks/>
        Error,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CopyIntoItemsLocal", WrapperNamespace="http://schemas.microsoft.com/sharepoint/soap/", IsWrapped=true)]
    public partial class CopyIntoItemsLocalRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=0)]
        public string SourceUrl;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=1)]
        public string[] DestinationUrls;
        
        public CopyIntoItemsLocalRequest() {
        }
        
        public CopyIntoItemsLocalRequest(string SourceUrl, string[] DestinationUrls) {
            this.SourceUrl = SourceUrl;
            this.DestinationUrls = DestinationUrls;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CopyIntoItemsLocalResponse", WrapperNamespace="http://schemas.microsoft.com/sharepoint/soap/", IsWrapped=true)]
    public partial class CopyIntoItemsLocalResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=0)]
        public uint CopyIntoItemsLocalResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=1)]
        public global::ccsvmadhocscan.CopySoap.CopyResult[] Results;
        
        public CopyIntoItemsLocalResponse() {
        }
        
        public CopyIntoItemsLocalResponse(uint CopyIntoItemsLocalResult, global::ccsvmadhocscan.CopySoap.CopyResult[] Results) {
            this.CopyIntoItemsLocalResult = CopyIntoItemsLocalResult;
            this.Results = Results;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CopyIntoItems", WrapperNamespace="http://schemas.microsoft.com/sharepoint/soap/", IsWrapped=true)]
    public partial class CopyIntoItemsRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=0)]
        public string SourceUrl;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=1)]
        public string[] DestinationUrls;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=2)]
        public global::ccsvmadhocscan.CopySoap.FieldInformation[] Fields;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=3)]
        [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] Stream;
        
        public CopyIntoItemsRequest() {
        }
        
        public CopyIntoItemsRequest(string SourceUrl, string[] DestinationUrls, global::ccsvmadhocscan.CopySoap.FieldInformation[] Fields, byte[] Stream) {
            this.SourceUrl = SourceUrl;
            this.DestinationUrls = DestinationUrls;
            this.Fields = Fields;
            this.Stream = Stream;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CopyIntoItemsResponse", WrapperNamespace="http://schemas.microsoft.com/sharepoint/soap/", IsWrapped=true)]
    public partial class CopyIntoItemsResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=0)]
        public uint CopyIntoItemsResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=1)]
        public global::ccsvmadhocscan.CopySoap.CopyResult[] Results;
        
        public CopyIntoItemsResponse() {
        }
        
        public CopyIntoItemsResponse(uint CopyIntoItemsResult, global::ccsvmadhocscan.CopySoap.CopyResult[] Results) {
            this.CopyIntoItemsResult = CopyIntoItemsResult;
            this.Results = Results;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetItem", WrapperNamespace="http://schemas.microsoft.com/sharepoint/soap/", IsWrapped=true)]
    public partial class GetItemRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=0)]
        public string Url;
        
        public GetItemRequest() {
        }
        
        public GetItemRequest(string Url) {
            this.Url = Url;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetItemResponse", WrapperNamespace="http://schemas.microsoft.com/sharepoint/soap/", IsWrapped=true)]
    public partial class GetItemResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=0)]
        public uint GetItemResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=1)]
        public global::ccsvmadhocscan.CopySoap.FieldInformation[] Fields;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.microsoft.com/sharepoint/soap/", Order=2)]
        [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] Stream;
        
        public GetItemResponse() {
        }
        
        public GetItemResponse(uint GetItemResult, global::ccsvmadhocscan.CopySoap.FieldInformation[] Fields, byte[] Stream) {
            this.GetItemResult = GetItemResult;
            this.Fields = Fields;
            this.Stream = Stream;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface CopySoapChannel : global::ccsvmadhocscan.CopySoap.CopySoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class CopySoapClient : System.ServiceModel.ClientBase<global::ccsvmadhocscan.CopySoap.CopySoap>, global::ccsvmadhocscan.CopySoap.CopySoap {
        
        public CopySoapClient() {
        }
        
        public CopySoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public CopySoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CopySoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CopySoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalResponse global::ccsvmadhocscan.CopySoap.CopySoap.CopyIntoItemsLocal(global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalRequest request) {
            return base.Channel.CopyIntoItemsLocal(request);
        }
        
        public uint CopyIntoItemsLocal(string SourceUrl, string[] DestinationUrls, out global::ccsvmadhocscan.CopySoap.CopyResult[] Results) {
            global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalRequest inValue = new global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalRequest();
            inValue.SourceUrl = SourceUrl;
            inValue.DestinationUrls = DestinationUrls;
            global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalResponse retVal = ((global::ccsvmadhocscan.CopySoap.CopySoap)(this)).CopyIntoItemsLocal(inValue);
            Results = retVal.Results;
            return retVal.CopyIntoItemsLocalResult;
        }
        
        public System.Threading.Tasks.Task<global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalResponse> CopyIntoItemsLocalAsync(global::ccsvmadhocscan.CopySoap.CopyIntoItemsLocalRequest request) {
            return base.Channel.CopyIntoItemsLocalAsync(request);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        global::ccsvmadhocscan.CopySoap.CopyIntoItemsResponse global::ccsvmadhocscan.CopySoap.CopySoap.CopyIntoItems(global::ccsvmadhocscan.CopySoap.CopyIntoItemsRequest request) {
            return base.Channel.CopyIntoItems(request);
        }
        
        public uint CopyIntoItems(string SourceUrl, string[] DestinationUrls, global::ccsvmadhocscan.CopySoap.FieldInformation[] Fields, byte[] Stream, out global::ccsvmadhocscan.CopySoap.CopyResult[] Results) {
            global::ccsvmadhocscan.CopySoap.CopyIntoItemsRequest inValue = new global::ccsvmadhocscan.CopySoap.CopyIntoItemsRequest();
            inValue.SourceUrl = SourceUrl;
            inValue.DestinationUrls = DestinationUrls;
            inValue.Fields = Fields;
            inValue.Stream = Stream;
            global::ccsvmadhocscan.CopySoap.CopyIntoItemsResponse retVal = ((global::ccsvmadhocscan.CopySoap.CopySoap)(this)).CopyIntoItems(inValue);
            Results = retVal.Results;
            return retVal.CopyIntoItemsResult;
        }
        
        public System.Threading.Tasks.Task<global::ccsvmadhocscan.CopySoap.CopyIntoItemsResponse> CopyIntoItemsAsync(global::ccsvmadhocscan.CopySoap.CopyIntoItemsRequest request) {
            return base.Channel.CopyIntoItemsAsync(request);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        global::ccsvmadhocscan.CopySoap.GetItemResponse global::ccsvmadhocscan.CopySoap.CopySoap.GetItem(global::ccsvmadhocscan.CopySoap.GetItemRequest request) {
            return base.Channel.GetItem(request);
        }
        
        public uint GetItem(string Url, out global::ccsvmadhocscan.CopySoap.FieldInformation[] Fields, out byte[] Stream) {
            global::ccsvmadhocscan.CopySoap.GetItemRequest inValue = new global::ccsvmadhocscan.CopySoap.GetItemRequest();
            inValue.Url = Url;
            global::ccsvmadhocscan.CopySoap.GetItemResponse retVal = ((global::ccsvmadhocscan.CopySoap.CopySoap)(this)).GetItem(inValue);
            Fields = retVal.Fields;
            Stream = retVal.Stream;
            return retVal.GetItemResult;
        }
        
        public System.Threading.Tasks.Task<global::ccsvmadhocscan.CopySoap.GetItemResponse> GetItemAsync(global::ccsvmadhocscan.CopySoap.GetItemRequest request) {
            return base.Channel.GetItemAsync(request);
        }
    }
}
