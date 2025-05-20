import {AxiosPromise, AxiosResponse} from 'axios';
import {API, coreAxiosInstance} from '@/shared/utils/axios-instance';
import { ReportDetails, DownloadReportParams } from '@/shared/models/iAM/reports';
import { UserDefinedReportRequestModel } from '@/shared/models/iAM/reports';

export default class ReportsService {
    static generateReport(scenarioId: string, reportType: string): AxiosPromise {
        return coreAxiosInstance.request({
            method: 'POST',
            url: `${API.Report}/GetFile/${reportType}`,
            headers: {'Content-Type': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'},
            data: scenarioId,
            responseType: 'text'
        });
    }
    /* TODO add param for userDefinedReportRequestModel type UserDefinedReportRequestModel */
    static generateReportWithCriteria(scenarioId: string, expression: string, reportType:string, userDefinedReportRequestModel: UserDefinedReportRequestModel): AxiosPromise {
        return coreAxiosInstance.request({
            method: 'POST',
            url: `${API.Report}/GetFile/${reportType}`,
            headers: {'Content-Type': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'},
            data: {'scenarioId':scenarioId, 'expression':expression, 'userDefinedReportRequestModel': userDefinedReportRequestModel},
            responseType: 'text'
        });
    }
    static downloadSimulationLog(networkId: string, scenarioId: string): AxiosPromise {
        return coreAxiosInstance.request({
            method: 'POST',
            url: `${API.SimulationLog}/GetSimulationLog/${networkId}/${scenarioId}`,
            headers: {'Content-Type': 'text'},
            responseType: 'arraybuffer'
        });
    }

    static async downloadReport(params: DownloadReportParams): Promise<void> {
        try {
            const response: AxiosResponse<Blob> = await coreAxiosInstance.get(
                `${API.Report}/DownloadReport/${params.scenarioId}/${params.reportName}`,
                {
                    responseType: 'blob', // IMPORTANT: This tells Axios to expect binary data
                }
            );

            // Create a URL for the blob
            const href = URL.createObjectURL(response.data);

            // Create a temporary anchor element and trigger download
            const link = document.createElement('a');
            link.href = href;

            // Try to get filename from Content-Disposition header
            let filenameFromServer: string | null = null;
            const disposition = response.headers['content-disposition'];

            if (disposition) {
                // More robust regex, handles quoted and unquoted, and filename*
                const filenameMatch = disposition.match(/filename\*?=(?:UTF-8'')?([^;\r\n]+|"[^"]*")/i);
                if (filenameMatch && filenameMatch[1]) {
                    filenameFromServer = decodeURIComponent(filenameMatch[1].replace(/^"|"$/g, ''));
                }
            }
            
            // Determine the final filename
            // If server sent a filename, use it. Otherwise, construct a default.
            const finalFilename = filenameFromServer || `${params.simulationName || params.scenarioId}_${params.reportName}${guessExtensionFromMimeType(response.headers['content-type'])}`;

            link.setAttribute('download', finalFilename);
            document.body.appendChild(link);
            link.click();

            // Clean up
            document.body.removeChild(link);
            URL.revokeObjectURL(href);

            function guessExtensionFromMimeType(mimeType?: string): string {
                if (!mimeType) {
                    return '.dat'; // Fallback if no MIME type header
                }
            
                // The actual MIME type is the part before any ';' (parameters like charset)
                const mainMimeType = mimeType.split(';')[0].trim().toLowerCase();
            
                switch (mainMimeType) {
                    case 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet':
                        return '.xlsx';
                    case 'application/json':
                        return '.json';

                    default:
                        return '.dat'; // Generic fallback for unmapped types
                }
            }

        } catch (error) {
            // Handle errors (e.g., network error, server error response)
            console.error('Failed to download report:', error);
            // addErrorNotificationAction({
            //     message: 'Failed to download report.',
            //     longMessage: error.response?.data?.message || error.message || 'An unknown error occurred.',
            // });
            throw error; // Re-throw to allow caller to handle if needed
        }
    }

    static getReportGenerationStatus(reportDetails: ReportDetails[]): AxiosPromise {
        return coreAxiosInstance.post(`${API.Report}/GetReportGenerationStatus`, reportDetails);
    }
    
    static deleteReport(scenarioId: string, reportName: string): AxiosPromise {
        return coreAxiosInstance.get(               
            `${API.Report}/DeleteReport/${scenarioId}/${reportName}`,
        );
    }

    static deleteAllGeneratedReports(scenarioId: string): AxiosPromise {
        return coreAxiosInstance.get(               
            `${API.Report}/DeleteAllGeneratedReports/${scenarioId}`,
        );
    }
}
