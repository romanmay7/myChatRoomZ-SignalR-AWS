import { Message } from "./message.model";

export class Channel {

  id: string;
  title: string;
  description: string;
  messageHistory: Message[]

}
