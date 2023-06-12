using System.Data.Entity;

namespace Inventory.Models
{
    public class InventoryModel:DbContext
    {
        public InventoryModel()
            : base("name=InventoryModel")
        {
        }
        public virtual DbSet<SalesPersonMaster> SalesPersonMasters { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<BOM> Bom { get; set; }
        public virtual DbSet<SOReplacement> SOReplacement { get; set; }
        public virtual DbSet<PurchaseOrderPayment> purchaseOrderPayment { get; set; }
        public virtual DbSet<SalesOrderPayment> SalesOrderPayment { get; set; }
        public virtual DbSet<TempSOReturn> tempSOReturn { get; set; }
        public virtual DbSet<TempDamage> TempDamage { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<ReturnReason> ReturnReason { get; set; }
        public virtual DbSet<ProductSerialNo> ProductSerialNo { get; set; }
        public virtual DbSet<Shipper> Shippers { get; set; }
        public virtual DbSet<TempSerialNo> TempSerialNo { get; set; }
        public virtual DbSet<TempSalesSerialNo> tempSalesSerialNo { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Paymentterms> Paymentterms { get; set; }
        public virtual DbSet<BankDetail> BankDetails { get; set; }
        public virtual DbSet<CompanyDetail> CompanyDetails { get; set; }
        public virtual DbSet<Suppliers> suppliers { get; set; }
        public virtual DbSet<TempTable> tempTable { get; set; }
        public virtual DbSet<TaxMaster> TaxMasters { get; set; }
        public virtual DbSet<POReturns> pOReturns { get; set; }
        public virtual DbSet<SupplierContacts> supplierContacts { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; }
        //public virtual DbSet<SupplierContacts> supplierContacts { get; set; }
        public virtual DbSet<CustomerContacts> CustomerContacts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<DiscountTypes> DiscountTypes { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<GRNDetails> GRNDetail { get; set; }
        public virtual DbSet<Damage> damage { get; set; }
        public virtual DbSet<SOReturns> sOReturns { get; set; }
        public virtual DbSet<POReplacement> POReplacement { get; set; }
        public virtual DbSet<BillNumbering> BillNumbering { get; set; }
        public virtual DbSet<DebitNote> DebitNote { get; set; }
        public virtual DbSet<CreditNote> CreditNote { get; set; }
        public System.Data.Entity.DbSet<Inventory.Models.SupplierProductRelations> SupplierProductRelations { get; set; }
        public System.Data.Entity.DbSet<Inventory.Models.CustomerProductRelations> CustomerProductRelations { get; set; }
        public virtual DbSet<Inventory.Models.POMain> POMains { get; set; }
        public virtual DbSet<Inventory.Models.OrderMain> orderMain { get; set; } 
        public virtual DbSet<Inventory.Models.PODetails> poDetails { get; set; }
        public virtual DbSet<Inventory.Models.OrderDetails> orderDetails { get; set; }
        public System.Data.Entity.DbSet<Inventory.Models.TermsAndConditions> TermsAndConditions { get; set; }
        public System.Data.Entity.DbSet<Inventory.Models.Sales> Sales { get; set; }
        public System.Data.Entity.DbSet<Inventory.Models.StoreLocations> StoreLocations { get; set; }
        public System.Data.Entity.DbSet<Inventory.Models.POInvoice> POInvoices { get; set; }
        public System.Data.Entity.DbSet<Inventory.Models.ExplodedBOM> ExplodedBOM { get; set; }
        public virtual DbSet<ProductionOrder> ProductionOrder { get; set; }
        public virtual DbSet<StockStatus> StockStatus { get; set; }       
        public DbSet<MRNMain> MRNMain { get; set; }
        public DbSet<MRNDetails> MRNDetails { get; set; }
        public DbSet<StockAllocation> StockAllocation { get; set; }

        public DbSet<IssueToProduction> IssueToProduction { get; set; }

        public DbSet<IssueToProductionDetails> IssueToProductionDetails { get; set; }

        public System.Data.Entity.DbSet<Inventory.Models.PRNMain> PRNMains { get; set; }
            
        //public System.Data.Entity.DbSet<Inventory.Models.Department> Department { get; set; }

        public System.Data.Entity.DbSet<Inventory.Models.PRNDetails> PRNDetails { get; set; }

        public System.Data.Entity.DbSet<Inventory.Models.Department> Departments { get; set; }

        public virtual DbSet<DoctorMasterModel>DoctorMasterModels { get; set; }

        //public virtual DbSet<ProductMaster> Product { get; set; }

        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }

        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<MMEMaster> MMEMaster { get; set; }

        public virtual DbSet<DiscountStructure>DiscountStructure { get; set; }
        public virtual DbSet<DCReturns> DCReturns { get; set; }

        public virtual DbSet<DCReplacement> DCReplacement { get; set; }
        //public virtual DbSet<Destination_details> Destination_details { get; set; }
        //public virtual DbSet<Origin_details> Origin_details { get; set; }
        //public virtual DbSet<Consignments> Consignments { get; set; }
    }
}