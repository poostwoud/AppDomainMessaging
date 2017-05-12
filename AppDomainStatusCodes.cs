namespace AppDomainMessaging
{
    public enum AppDomainStatusCodes
    {

        // ReSharper disable InconsistentNaming
        OK = 200,
        // ReSharper restore InconsistentNaming

        Created = 201,
        
        /// <summary>
        /// The server successfully processed the request and is not returning any content.
        /// </summary>
        NoContent = 204,


        NotFound = 404,


        InternalServerError = 500
    }
}