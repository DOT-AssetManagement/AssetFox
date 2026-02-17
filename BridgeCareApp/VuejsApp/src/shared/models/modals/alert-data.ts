export interface AlertData {
    showDialog: boolean;
    heading: string;
    message: string;
    choice: boolean;
}

export const emptyAlertData: AlertData = {
    showDialog: false,
    heading: '',
    message: '',
    choice: false
};

export interface AlertPreChecksData {
    showDialog: boolean;
    ErrorHeading: string;
    ErrorMessage: string;
    WarningHeading: string;
    WarningMessage: string;
    SuccessHeading: string;
    SuccessMessage: string;
    choice: boolean;
}

export const emptyAlertPreChecksData: AlertPreChecksData = {
    showDialog: false,
    ErrorHeading: '',
    ErrorMessage: '',
    WarningHeading: '',
    WarningMessage: '',
    SuccessHeading: '',
    SuccessMessage: '',
    choice: false
};

export interface PreChecksValidationLog {
    ErrorMessages: string[],
    WarningMessages: string[]
}

export const emptyPreChecksValidationLog : PreChecksValidationLog = {
    ErrorMessages: [],
    WarningMessages: []
}

export interface AlertDataWithButtons {
    showDialog: boolean;
    heading: string;
    message: string;
    choice: boolean;
    buttons: string[];
}

export const emptyAlertDataWithButtons: AlertDataWithButtons = {
    showDialog: false,
    heading: '',
    message: '',
    choice: false,
    buttons : ['']
};

export interface AlertButton {
    label: string;
}

export const emptyAlertButton: AlertButton = {
    label: '',
}